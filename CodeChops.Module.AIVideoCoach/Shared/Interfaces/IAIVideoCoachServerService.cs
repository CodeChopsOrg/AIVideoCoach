using System.Collections.Generic;
using System.Threading.Tasks;
using Trifoia.Module.AIVideoCoach.Models;

namespace Trifoia.Module.AIVideoCoach.Services
{
    public interface IAIVideoCoachServerService
    {
        Task<(double, TextEmbedding)> EmbedQueryAsync(string query);

        Task<bool> EmbedTextAsync(TextEmbedding textEmbedding);

        Task<bool> PostEmbeddingsAsync(TextEmbedding textEmbedding);

        Task<(double, TextEmbedding)> FindMostRelevantChunkAsync(float[] requestEmbedding);

        double CalculateCosineSimilarity(float[] vector1, float[] vector2);
    }
}
