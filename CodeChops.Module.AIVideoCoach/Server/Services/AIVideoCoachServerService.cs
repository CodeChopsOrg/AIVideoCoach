using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Oqtane.Modules;
using CodeChops.Module.AIVideoCoach.Models;
using CodeChops.Module.AIVideoCoach.Repository;
using CodeChops.Module.AIVideoCoach.Services;
using CodeChops.Module.AIVideoCoach.Shared.Interfaces;

namespace CodeChops.Module.AIVideoCoach.Services
{
    public class AIVideoCoachServerService : IAIVideoCoachServerService, ITransientService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration configuration;
        private readonly AIVideoCoachRepository _AIVideoCoachRepository;
        private readonly IEmbeddingClient _embeddingClient;

        public AIVideoCoachServerService(IChatClientFactory<IEmbeddingClient> embeddingClientFactory, HttpClient httpClient, IConfiguration configuration, AIVideoCoachRepository AIVideoCoachRepository)
        {
            _embeddingClient = embeddingClientFactory.Create();
            _httpClient = httpClient;
            this.configuration = configuration;
            _AIVideoCoachRepository = AIVideoCoachRepository;
        }
        public async Task<(double, TextEmbedding)> EmbedQueryAsync(string query)
        {
            var embeddingClient = configuration["AIHost"];
            EmbeddingResponse embeddingResponse;

            if (embeddingClient == "local")
            {
                var requestBody = new
                {
                    model = "mxbai-embed-large",
                    prompt = query
                };

                var response = await _httpClient.PostAsJsonAsync("http://localhost:11434/api/embeddings", requestBody);
                var responseContent = await response.Content.ReadAsStringAsync();

                embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(responseContent);

                if (embeddingResponse == null || embeddingResponse.Embedding == null)
                {
                    throw new Exception("Failed to generate query embedding.");
                }
            }
            else
            {
                embeddingResponse = await _embeddingClient.GenerateEmbeddingsAsync(query);
            }

            (var similarity, var text) = await FindMostRelevantChunkAsync(embeddingResponse.Embedding);

            return (similarity, text);
        }

        public async Task<bool> EmbedTextAsync(TextEmbedding textEmbedding)
        {
            if (textEmbedding == null || string.IsNullOrWhiteSpace(textEmbedding.Text))
            {
                throw new ArgumentException("TextEmbedding object cannot be null, and Text cannot be empty.", nameof(textEmbedding));
            }

            var embeddingClient = configuration["AIHost"];

            EmbeddingResponse embeddingResponse;

            if (embeddingClient == "local")
            {
                var requestBody = new
                {
                    model = "mxbai-embed-large",
                    prompt = textEmbedding.Text
                };

                var response = await _httpClient.PostAsJsonAsync("http://localhost:11434/api/embeddings", requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Ollama embedding API failed with status code: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(responseContent);

                if (embeddingResponse == null || embeddingResponse.Embedding == null)
                {
                    throw new Exception("Failed to generate embeddings.");
                }
            }
            else
            {
                embeddingResponse = await _embeddingClient.GenerateEmbeddingsAsync(textEmbedding.Text);
            }

            textEmbedding.Embedding = JsonSerializer.Serialize(embeddingResponse.Embedding);

            var result = await PostEmbeddingsAsync(textEmbedding);

            if (!result)
            {
                throw new Exception($"Failed to save embedding to the database.");
            }
            return result;
        }

        public async Task<(double, TextEmbedding)> FindMostRelevantChunkAsync(float[] requestEmbedding)
        {
            // Retrieve all stored embeddings from the repository
            var storedEmbeddings = await _AIVideoCoachRepository.GetEmbeddedChunksAsync();

            if (storedEmbeddings == null || !storedEmbeddings.Any())
            {
                throw new Exception("No stored embeddings found.");
            }

            // Compare the request embedding with stored embeddings
            TextEmbedding mostRelevantChunk = null;
            double highestSimilarity = double.MinValue;

            foreach (var embedding in storedEmbeddings)
            {
                var storedEmbedding = JsonSerializer.Deserialize<float[]>(embedding.Embedding);

                // Calculate cosine similarity
                double similarity = CalculateCosineSimilarity(requestEmbedding, storedEmbedding);

                // Update the most relevant chunk if this one has higher similarity
                if (similarity > highestSimilarity)
                {
                    highestSimilarity = similarity;
                    mostRelevantChunk = embedding;
                }
            }

            // Return the most relevant chunk
            return (highestSimilarity, mostRelevantChunk);
        }

        public double CalculateCosineSimilarity(float[] vector1, float[] vector2)
        {
            if (vector1.Length != vector2.Length)
            {
                return 0;
            }

            double dotProduct = 0, magnitude1 = 0, magnitude2 = 0;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += Math.Pow(vector1[i], 2);
                magnitude2 += Math.Pow(vector2[i], 2);
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0 || magnitude2 == 0)
                return 0;

            return dotProduct / (magnitude1 * magnitude2);
        }

        public async Task<bool> PostEmbeddingsAsync(TextEmbedding embedding)
        {
            if (embedding is not { Text: { Length: > 0 }, Embedding: { Length: > 0 } })
            {
                throw new ArgumentException("Invalid TextEmbedding object.");
            }
            try
            {
                var success = await _AIVideoCoachRepository.AddEmbedding(embedding);
                return success;
            }
            catch (Exception)
            {
                throw new Exception("Failed to post Embedding");
            }
        }
    }
}
