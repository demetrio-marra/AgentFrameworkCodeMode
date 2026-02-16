namespace AgentFrameworkCodeMode.Infrastructure.Sandbox
{
    public class SESJSSandboxConfiguration
    {
        public string McpServerHost { get; set; } = string.Empty;
        public string MCPServerAgentId { get; set; } = string.Empty;
        public string? NodeExtraCACertsPath { get; set; }
    }
}
