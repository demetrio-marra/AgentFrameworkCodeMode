using AgentFrameworkCodeMode.Models.Embedding;
using AgentFrameworkCodeMode.Models.Skills;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace AgentFrameworkCodeMode.Infrastructure.Skills
{
    public class QDrantSkillsFinder : ISkillsFinder
    {
        private readonly QdrantClient _qdrantClient;
        private readonly IEmbeddingService _embeddingService;

        public QDrantSkillsFinder(QDrantConfiguration configuration,
            IEmbeddingService embeddingService)
        {
            _embeddingService = embeddingService;
            _qdrantClient = new QdrantClient(
               host: configuration.Host,
               https: configuration.Https,
               port: configuration.Port
           );
           
        }

        public async Task<IEnumerable<string>> GetAvailableSkillsAsync(IEnumerable<string> actionableRequirements,
            CancellationToken cancellationToken = default)
        {
            var results = new List<string>();

            var embeddings = await _embeddingService.GetEmbeddingAsync(actionableRequirements);
            var searchPoints = embeddings.Select(vec => new SearchPoints
            {
                WithPayload = true,
                WithVectors = false,
                Limit = 10
            }).ToList();

            var li = 0;
            foreach (var emb in embeddings)
            {
                searchPoints[li].Vector.AddRange(emb);
                li++;
            }

            var batchSearchResult = await _qdrantClient.SearchBatchAsync(
                collectionName: "BusinessProcesses",
                searches: searchPoints,
                cancellationToken: cancellationToken
            );

            var rr = new List<ResultWithRelevance>();
            foreach (var searchResult in batchSearchResult)
            {
                foreach (var result in searchResult.Result)
                {
                    if (result.Payload.TryGetValue("text", out var extractedText))
                    {
                        rr.Add(new ResultWithRelevance
                        {
                            ResultText = extractedText.ToString(),
                            Relevance = result.Score
                        });
                    }
                }
            }

            results = rr.OrderByDescending(r => r.Relevance)
                .Take(5)
                .Select(r => r.ResultText)
                .Distinct()
                .ToList();

            return results;
        }

        public async Task<IEnumerable<string>> GetAvailableSkillsAsync(IEnumerable<string> actionableRequirements, 
            Dictionary<string, string> filters,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private class ResultWithRelevance
        {
            public string ResultText { get; set; } = string.Empty;
            public float Relevance { get; set; }
        }
    }
}
