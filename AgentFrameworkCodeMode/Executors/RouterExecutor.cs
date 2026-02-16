using AgentFrameworkCodeMode.Models.StructuredOutputs;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgentFrameworkCodeMode.Executors
{
    internal class RouterExecutor : Executor<ContextAnalyzerAgentOutput, RouterAgentOutput>
    {
        private readonly AIAgent _agent;

        public RouterExecutor(AIAgent agent) : base("Router")
        {
            _agent = agent;
        }

        public override async ValueTask<RouterAgentOutput> HandleAsync(ContextAnalyzerAgentOutput contextAnalyzerOutput, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            var originalRequestByUser = await context.ReadStateAsync<string>(WorkflowConstants.WORKFLOW_ORIGINAL_REQUEST_BY_USER_KEY, scopeName: WorkflowConstants.WORKFLOW_DEFAULT_SCOPE_KEY, cancellationToken: cancellationToken);
            
            // Construct the messages for the agent
            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, $"The user has the following request: {originalRequestByUser}"),
            };

            if (contextAnalyzerOutput.KeyFacts.Any())
            {
                messages.Add(new ChatMessage(ChatRole.User, $"The context analyzer agent has extracted the following key facts: {string.Join(", ", contextAnalyzerOutput.KeyFacts)}."));
            }

            // Invoke the agent
            var response = await this._agent.RunAsync(messages, cancellationToken: cancellationToken);
            var ret = JsonSerializer.Deserialize<RouterAgentOutput>(response.Text, JsonSerializerOptions.Web);
            return ret!;
        }
    }
}
