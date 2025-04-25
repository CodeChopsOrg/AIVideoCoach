using CodeChops.Module.AIVideoCoach.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CodeChops.Module.AIVideoCoach.Models
{
    public class TextEmbedding
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? VideoUrl { get; set; }
        public TimeSpan? StartTime { get; set; }
        public string Text { get; set; }
        public string Embedding { get; set; }
    }

    public class EmbeddingResponse
    {
        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; }

        [JsonPropertyName("data")]
        public List<EmbeddingData> Data { get; set; }
    }

    public class EmbedQueryRequest
    {
        public double Similarity { get; set; } 
        public TextEmbedding Embedding { get; set; }
    }

    public class EmbeddingData
    {
        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }
    }
}
