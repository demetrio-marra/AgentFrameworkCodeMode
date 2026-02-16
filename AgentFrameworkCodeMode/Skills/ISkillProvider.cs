namespace AgentFrameworkCodeMode.Skills
{
    internal interface ISkillProvider
    {
        Task<IEnumerable<string>> GetAvailableSkillsAsync(CancellationToken cancellationToken = default);
        Task<string> GetSkillAsync(string skillName, string agentName, CancellationToken cancellationToken = default);
    }
}
