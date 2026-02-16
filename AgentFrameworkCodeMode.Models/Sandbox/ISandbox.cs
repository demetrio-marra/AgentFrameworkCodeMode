namespace AgentFrameworkCodeMode.Models.Sandbox
{
    public interface ISandbox
    {
        Task<SandboxOutput> ExecuteCodeAsync(string agentId, string code, CancellationToken cancellationToken = default);
    }
}
