using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Modules;
using Oqtane.Shared;
using System.Net;
using Microsoft.Extensions.AI;
using System;
using System.Text;
using CodeChops.Module.AIVideoCoach.Models;
using CodeChops.Module.AIVideoCoach.Shared.Interfaces;
using Oqtane.Services;



namespace CodeChops.Module.AIVideoCoach.Services
{
    public class AIVideoCoachService : ResponseServiceBase, IService
    {
        private IChatClient _client;
        private string Apiurl => CreateApiUrl("AIVideoCoach");

        public AIVideoCoachService(IHttpClientFactory http, SiteState siteState) : base(http, siteState)
        {

        }

        public async Task InitializeAsync(IChatClientFactory<IChatClient> factory)
        {
            _client = await factory.CreateAsync();

        }

        /// <summary>
        /// Streaming chat.
        /// </summary>
        /// <param name="request">Initial request with history.</param>
        /// <returns>Stream of agent responses.</returns>
        internal async IAsyncEnumerable<string> Stream(ChatRequest request)
        {
            var history = CreateHistoryFromRequest(request);
            IAsyncEnumerable<StreamingChatCompletionUpdate> response = _client.CompleteStreamingAsync(history);

            await foreach (StreamingChatCompletionUpdate content in response)
            {
                yield return content.Text ?? string.Empty;
            }
        }

        /// <summary>
        /// Map from our UI entity to the Extensions.AI version.
        /// </summary>
        /// <param name="message">The UI message to convert.</param>
        /// <returns>The equivalent <see cref="ChatMessage"/>.</returns>
        private static Microsoft.Extensions.AI.ChatMessage ConvertMessage(Message message)
        {
            var textMessage = new Microsoft.Extensions.AI.ChatMessage(
                message.IsAssistant ? ChatRole.Assistant : ChatRole.User,
                message.Context);

            return textMessage;
        }

        /// <summary>
        /// Rebuilds the history for the chat client.
        /// </summary>
        /// <param name="request">Request with history.</param>
        /// <returns>Converted history.</returns>
        private static IList<Microsoft.Extensions.AI.ChatMessage> CreateHistoryFromRequest(ChatRequest request)
        {
            var history = new List<Microsoft.Extensions.AI.ChatMessage>([new Microsoft.Extensions.AI.ChatMessage(
            ChatRole.System,
            "You are a helpful assistant designed to answer questions asked by people taking a video course series. Use the provided context to answer questions concisely and accurately without explicitly referencing the context.")]);

            foreach (Message message in request.Messages)
            {
                history.Add(ConvertMessage(message));
            }

            return history;
        }

        public async Task ChunkText(TextEmbedding textEmbedding)
        {
            if (textEmbedding == null || string.IsNullOrWhiteSpace(textEmbedding.Text))
            {
                throw new ArgumentException("Invalid TextEmbedding object or empty text content.");
            }

            // Set the chunk size
            const int chunkSize = 1000;

            var text = textEmbedding.Text;
            int startIndex = 0;

            while (startIndex < text.Length)
            {
                int endIndex = Math.Min(startIndex + chunkSize, text.Length);
                string chunk = text.Substring(startIndex, endIndex - startIndex);

                // Create a new TextEmbedding object for the chunk
                var chunkEmbedding = new TextEmbedding
                {
                    Title = textEmbedding.Title,
                    Text = chunk,
                    Embedding = null // This can be set if embedding is performed immediately
                };

                // Process the chunk with the embedding method
                await EmbedTextAsync(chunkEmbedding);

                startIndex = endIndex;
            }
        }


        /// <summary>
        /// Chunk the text from a VTT file and embed each chunk using Ollama's embedding API.
        /// </summary>
        /// <param name="vttEmbedding"></param>
        /// <returns></returns>
        public async Task ChunkTextFromVttContentAsync(TextEmbedding vttEmbedding)
        {
            if (vttEmbedding == null || string.IsNullOrWhiteSpace(vttEmbedding.Text))
            {
                throw new ArgumentException("Invalid VTT embedding object or empty text content.");
            }

            var lines = vttEmbedding.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var chunks = new List<(TimeSpan StartTime, string Text)>();
            StringBuilder currentChunkText = new StringBuilder();
            TimeSpan currentChunkStartTime = TimeSpan.Zero;

            foreach (var line in lines)
            {
                // Remove the WEBVTT header
                if (line.Equals("WEBVTT", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Remove lines that are only numbers
                if (int.TryParse(line.Trim(), out _))
                {
                    continue;
                }
                // Check for timestamp lines (format: 00:00:00.000 --> 00:00:01.000)
                if (line.Contains("-->"))
                {
                    // Parse the starting timestamp
                    var timestamp = line.Split("-->")[0].Trim();
                    if (TimeSpan.TryParse(timestamp, out var startTime))
                    {
                        // If a new minute starts, finalize the previous chunk
                        if (startTime.Minutes > currentChunkStartTime.Minutes || startTime.Hours > currentChunkStartTime.Hours)
                        {
                            if (currentChunkText.Length > 0)
                            {
                                chunks.Add((currentChunkStartTime, currentChunkText.ToString().Trim()));
                                currentChunkText.Clear();
                            }
                            currentChunkStartTime = startTime;
                        }
                    }

                    // Skip adding the timestamp line to the chunk
                    continue;
                }

                // Remove speaker tags or other special tags (e.g., <v ->)
                var cleanedLine = System.Text.RegularExpressions.Regex.Replace(line, "<.*?>", "").Trim();

                // Skip empty lines after cleaning
                if (string.IsNullOrWhiteSpace(cleanedLine))
                {
                    continue;
                }

                // Accumulate text into the current chunk
                currentChunkText.AppendLine(cleanedLine);
            }

            // Add the last chunk if it exists
            if (currentChunkText.Length > 0)
            {
                chunks.Add((currentChunkStartTime, currentChunkText.ToString().Trim()));
            }

            // Process each chunk
            foreach (var chunk in chunks)
            {
                // Create a new TextEmbedding for each chunk with StartTime set
                var chunkEmbedding = new TextEmbedding
                {
                    Title = vttEmbedding.Title,
                    VideoUrl = vttEmbedding.VideoUrl,
                    StartTime = chunk.StartTime,
                    Text = chunk.Text,
                    Embedding = null // Embed it here if needed
                };

                // Call your embedding method
                await EmbedTextAsync(chunkEmbedding);
            }
        }

        public async Task<(TextEmbedding, HttpStatusCode)> EmbedTextAsync(TextEmbedding embedding)
        {
            var url = $"{Apiurl}/save-embedding";

            (var data, var response) = await PostJsonWithResponseAsync(url, embedding);

            return (data, response.StatusCode);
        }


        public async Task<(EmbedQueryRequest, HttpStatusCode)> EmbedQueryAsync(string query)
        {
            var url = $"{Apiurl}/embed-query";

            // Create the request payload
            var requestPayload = new EmbedQueryRequest
            {
                Similarity = 0.0, // Default similarity value
                Embedding = new TextEmbedding { Text = query } // Set the query text
            };

            // Send the request and await the response
            var responseTuple = await PostJsonWithResponseAsync<EmbedQueryRequest>(url, requestPayload);

            // Manually deconstruct response
            var data = responseTuple.Item1;
            var response = responseTuple.Item2;

            if (response.IsSuccessStatusCode && data is not null)
            {
                // Return the data tuple and status code
                return (data, response.StatusCode);
            }

            // Return a default tuple on failure
            return (new EmbedQueryRequest
            {
                Similarity = 0.0, // Default similarity value
                Embedding = new TextEmbedding { Text = query } // Set the query text
            }, response.StatusCode);
        }

        public async Task<(List<TextEmbedding> Embeddings, HttpStatusCode Code)> GetEmbeddedChunksAsync()
        {
            var url = $"{Apiurl}/get-embedded-chunks";

            (var data, var response) = await GetJsonWithResponseAsync<List<TextEmbedding>>(url);

            return (data, response.StatusCode);
        }


        public async Task<HttpStatusCode> DeleteEmbeddingsAsync(string Title)
        {
            if (string.IsNullOrWhiteSpace(Title))
                throw new ArgumentException("Video title cannot be null or empty.", nameof(Title));

            // Construct the DELETE request URL
            var url = $"{Apiurl}/delete-embeddings/{Title}";

            // Use the DeleteWithResponseAsync method
            var response = await DeleteWithResponseAsync(url);

            return response.StatusCode;
        }

    }
}