using Microsoft.Agents.AI;

namespace AgentFrameworkCodeMode.Models
{
    internal interface IAgentFactory
    {
        Task<AIAgent> CreateAgentAsync(string agentName);
    }
}
