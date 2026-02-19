using AgentFrameworkCodeMode.Models.Sandbox;

namespace AgentFrameworkCodeMode.Infrastructure.Sandbox
{
    public class SESJSSandboxDocumentationProvider : ISandboxDocumentationProvider
    {
        public async Task<Dictionary<string, string>> GetDocumentationAsync(IEnumerable<string> documentationKeys, CancellationToken cancellationToken = default)
        {
            var documentation = new Dictionary<string, string>();

            foreach (var documentationKey in documentationKeys)
            {
                var filePath = Path.Combine("Sandbox", "Documentation", $"{documentationKey}.txt");

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Documentation file not found: {filePath}", filePath);
                }

                documentation[documentationKey] = await File.ReadAllTextAsync(filePath, cancellationToken);
            }
            
            return documentation;
        }
    }
}
