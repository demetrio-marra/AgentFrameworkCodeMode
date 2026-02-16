using AgentFrameworkCodeMode.Models.Inputs;
using AgentFrameworkCodeMode.Models.StructuredOutputs;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgentFrameworkCodeMode.Executors
{
    internal class ContextAnalyzerExecutor : Executor<ContextAnalyzerInput, ContextAnalyzerAgentOutput>
    {
        private readonly AIAgent _agent;

        public ContextAnalyzerExecutor(AIAgent agent) : base("ContextAnalyzer")
        {
            _agent = agent;
        }

        public override async ValueTask<ContextAnalyzerAgentOutput> HandleAsync(ContextAnalyzerInput input, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            await context.QueueStateUpdateAsync(WorkflowConstants.WORKFLOW_ORIGINAL_REQUEST_BY_USER_KEY, input.RequestByUser, scopeName: WorkflowConstants.WORKFLOW_DEFAULT_SCOPE_KEY, cancellationToken: cancellationToken);

            // Prepare messages for the agent
            var messages = input.History.Select(i => new Microsoft.Extensions.AI.ChatMessage(i.Role, i.Text)).ToList();
            messages.Add(new ChatMessage(ChatRole.User, input.RequestByUser));

            // Invoke the agent
            try
            {
                var response = await this._agent.RunAsync(messages, cancellationToken: cancellationToken);
                var ret = JsonSerializer.Deserialize<ContextAnalyzerAgentOutput>(response.Text, JsonSerializerOptions.Web);

                await context.QueueStateUpdateAsync(WorkflowConstants.WORKFLOW_CONTEXT_KEY_FACTS_KEY, 
                    ret.KeyFacts,
                    scopeName: WorkflowConstants.WORKFLOW_DEFAULT_SCOPE_KEY, 
                    cancellationToken: cancellationToken);

                return ret;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to invoke ContextAnalyzer agent. Messages count: {messages.Count}, Input: '{input.RequestByUser}'", ex);
            }
        }
    }
}
