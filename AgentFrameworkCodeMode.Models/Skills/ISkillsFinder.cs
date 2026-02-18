namespace AgentFrameworkCodeMode.Models.Skills
{
    public interface ISkillsFinder
    {
        /// <summary>
        /// Retrieves the available skills that match the given requirements.
        /// </summary>
        /// <param name="actionableRequirements">The requirements that need to be fulfilled by the available skills.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A collection of available skills that match the given requirements.</returns>
        Task<IEnumerable<string>> GetAvailableSkillsAsync(IEnumerable<string> actionableRequirements, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the available skills that match the given requirements and filters.
        /// </summary>
        /// <param name="actionableRequirements">The requirements that need to be fulfilled by the available skills.</param>
        /// <param name="filters">A dictionary of filters to apply when searching for skills.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A collection of available skills that match the given requirements and filters.</returns>
        Task<IEnumerable<string>> GetAvailableSkillsAsync(IEnumerable<string> actionableRequirements, Dictionary<string, string> filters, CancellationToken cancellationToken = default);
    }
}
