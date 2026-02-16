using AgentFrameworkCodeMode.Models.StructuredOutputs;
using AgentFrameworkCodeMode.Skills;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgentFrameworkCodeMode.Executors
{
    internal class BusinessAdvisorExecutor : Executor<RouterAgentOutput, BusinessAdvisorAgentOutput>
    {
        private const string AGENT_NAME = "BusinessAdvisor";

        private readonly AIAgent _agent;
        private readonly ISkillProvider _skillProvider;

        public BusinessAdvisorExecutor(AIAgent agent, ISkillProvider skillProvider) : base(AGENT_NAME)
        {
            _agent = agent;
            _skillProvider = skillProvider;
        }

        public override async ValueTask<BusinessAdvisorAgentOutput> HandleAsync(RouterAgentOutput input, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            var originalRequestByUser = await context.ReadStateAsync<string>(WorkflowConstants.WORKFLOW_ORIGINAL_REQUEST_BY_USER_KEY, scopeName: WorkflowConstants.WORKFLOW_DEFAULT_SCOPE_KEY, cancellationToken: cancellationToken);
            var listOfKeyFacts = await context.ReadStateAsync<List<string>>(WorkflowConstants.WORKFLOW_CONTEXT_KEY_FACTS_KEY, scopeName: WorkflowConstants.WORKFLOW_DEFAULT_SCOPE_KEY, cancellationToken: cancellationToken);

            // fixed per ora
            var skill = "Statistics";
            var skillDetails = await _skillProvider.GetSkillAsync(skill, AGENT_NAME, cancellationToken);

            var messages = new List<ChatMessage>();

            messages.Add(new ChatMessage(ChatRole.System, $"Additional documentation:\n{skillDetails}"));

            messages.Add(new ChatMessage(ChatRole.User, $"The user has the following request: {originalRequestByUser}"));

            if (listOfKeyFacts.Any())
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
