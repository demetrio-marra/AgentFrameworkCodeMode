namespace AgentFrameworkCodeMode.Models.Embedding
{
    public interface IEmbeddingService
    {
        Task<float[]> GetEmbeddingAsync(string input, CancellationToken cancellationToken = default);
        Task<IEnumerable<float[]>> GetEmbeddingAsync(IEnumerable<string> inputs, CancellationToken cancellationToken = default);
    }
}
