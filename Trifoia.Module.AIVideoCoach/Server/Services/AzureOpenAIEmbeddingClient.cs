using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Trifoia.Module.AIVideoCoach.Services;
using System.Text.Json;
using System.Net.Http.Json;
using Trifoia.Module.AIVideoCoach.Models;
using Oqtane.Services;

namespace CodeChops.Module.AIVideoCoach.Services
{
    using Oqtane.Repository;
    using Oqtane.Shared;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;

    public class AzureOpenAIEmbeddingClient : IEmbeddingClient
    {
        private readonly HttpClient _httpClient;
        private readonly Uri _endpoint;


        public AzureOpenAIEmbeddingClient(Uri endpoint, string deploymentId, string apiKey)
        {
            _endpoint = new Uri($"{endpoint}/openai/deployments/{deploymentId}/embeddings?api-version=2023-05-15");
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
        }

        public async Task<EmbeddingResponse> GenerateEmbeddingsAsync(string inputText)
        {
            var payload = new { input = inputText };
            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_endpoint, jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Azure OpenAI API failed: {response.StatusCode}, {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<EmbeddingResponse>(content);
            var embeddingResult = new EmbeddingResponse
            {
                Embedding = result.Data[0].Embedding,
            };

            return embeddingResult ?? throw new Exception("Embedding response is invalid.");
        }
    }
}
