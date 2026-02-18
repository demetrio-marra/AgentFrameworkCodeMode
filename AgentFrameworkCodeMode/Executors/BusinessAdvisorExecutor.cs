using AgentFrameworkCodeMode.Models.Skills;
using AgentFrameworkCodeMode.Models.StructuredOutputs;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgentFrameworkCodeMode.Executors
{
    internal class BusinessAdvisorExecutor : Executor<RouterAgentOutput, BusinessAdvisorAgentOutput>
    {
        private const string AGENT_NAME = "BusinessAdvisor";

        private readonly AIAgent _agent;
        private readonly ISkillsFinder _skillsFinder;

        public BusinessAdvisorExecutor(AIAgent agent, ISkillsFinder skillsFinder) : base(AGENT_NAME)
        {
            _agent = agent;
            _skillsFinder = skillsFinder;
        }

        public override async ValueTask<BusinessAdvisorAgentOutput> HandleAsync(RouterAgentOutput input, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            var originalRequestByUser = await context.ReadStateAsync<string>(WorkflowConstants.WORKFLOW_ORIGINAL_REQUEST_BY_USER_KEY, scopeName: WorkflowConstants.WORKFLOW_DEFAULT_SCOPE_KEY, cancellationToken: cancellationToken);
            var listOfKeyFacts = await context.ReadStateAsync<List<string>>(WorkflowConstants.WORKFLOW_CONTEXT_KEY_FACTS_KEY, scopeName: WorkflowConstants.WORKFLOW_DEFAULT_SCOPE_KEY, cancellationToken: cancellationToken);
            var listOfActionableRequirements = await context.ReadStateAsync<List<string>>(WorkflowConstants.WORKFLOW_CONTEXT_ACTIONABLE_REQUIREMENTS_KEY, scopeName: WorkflowConstants.WORKFLOW_DEFAULT_SCOPE_KEY, cancellationToken: cancellationToken);

            var messages = new List<ChatMessage>();

            if (listOfActionableRequirements != null
                && listOfActionableRequirements.Any())
            {
                var skillDetails = await _skillsFinder.GetAvailableSkillsAsync(listOfActionableRequirements, cancellationToken);
                var joinedSkillDetails = string.Join("\n\n\n", skillDetails.Select(sd => sd));
                messages.Add(new ChatMessage(ChatRole.System, $"Additional documentation:\n{joinedSkillDetails}"));
            }

            messages.Add(new ChatMessage(ChatRole.User, $"The user has the following request: {originalRequestByUser}"));

            if (listOfKeyFacts != null
                && listOfKeyFacts.Any())
            {
                messages.Add(new ChatMessage(ChatRole.User, $"The context analyzer agent has extracted the following key facts: {string.Join(", ", listOfKeyFacts)}."));
            }

            // Invoke the agent
            var response = await this._agent.RunAsync(messages, cancellationToken: cancellationToken);
            
            var ret = new BusinessAdvisorAgentOutput
            {
                Documentation = response.Text
            };

            return ret!;
        }
    }
}
