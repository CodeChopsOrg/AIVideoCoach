using System.Threading.Tasks;
using Trifoia.Module.AIVideoCoach.Models;

namespace Trifoia.Module.AIVideoCoach.Services
{
    public interface IEmbeddingClient
    {
        Task<EmbeddingResponse> GenerateEmbeddingsAsync(string inputText);
    }
}
