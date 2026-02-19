namespace AgentFrameworkCodeMode.Models.Sandbox
{
    public interface ISandboxDocumentationProvider
    {
        Task<Dictionary<string, string>> GetDocumentationAsync(IEnumerable<string> documentationKeys, CancellationToken cancellationToken = default);
    }
}
