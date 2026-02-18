namespace AgentFrameworkCodeMode.Models.Embedding
{
    public interface IEmbeddingService
    {
        Task<float[]> GetEmbeddingAsync(string input, CancellationToken cancellationToken = default);
    }
}
