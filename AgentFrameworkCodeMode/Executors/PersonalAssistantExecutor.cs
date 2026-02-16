using AgentFrameworkCodeMode.Models.Inputs;
using AgentFrameworkCodeMode.Models.StructuredOutputs;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgentFrameworkCodeMode.Executors
{
    internal sealed partial class PersonalAssistantExecutor(AIAgent agent) : Executor("PersonalAssistant")
    {
        private const string USER_REQUEST_TEMPLATE = "The user stated:\n{0}";
        private const string KEY_FACTS_TEMPLATE = "Those are relevant key facts about the user's statement:\n{0}.";

        private readonly AIAgent _agent = agent;

        [MessageHandler]
        public async ValueTask<string> HandleAsync(RouterAgentOutput input, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            var originalRequestByUser = await context.ReadStateAsync<string>(WorkflowConstants.WORKFLOW_ORIGINAL_REQUEST_BY_USER_KEY, scopeName: WorkflowConstants.WORKFLOW_DEFAULT_SCOPE_KEY, cancellationToken: cancellationToken);
            var keyFacts = await context.ReadStateAsync<List<string>>(WorkflowConstants.WORKFLOW_CONTEXT_KEY_FACTS_KEY, scopeName: WorkflowConstants.WORKFLOW_DEFAULT_SCOPE_KEY, cancellationToken: cancellationToken);

            // Construct the messages for the agent
            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, string.Format(USER_REQUEST_TEMPLATE, originalRequestByUser)),
            };

            if (keyFacts.Any())
            {
                messages.Add(new ChatMessage(ChatRole.User, string.Format(KEY_FACTS_TEMPLATE, string.Join(", ", keyFacts))));
            }

            // Invoke the agent
            var response = await this._agent.RunAsync(messages, cancellationToken: cancellationToken);

            return response.Text;
        }


        [MessageHandler]
        public async ValueTask<string> HandleAsync(CodeSandboxOutput input, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            var originalRequestByUser = await context.ReadStateAsync<string>(WorkflowConstants.WORKFLOW_ORIGINAL_REQUEST_BY_USER_KEY, scopeName: WorkflowConstants.WORKFLOW_DEFAULT_SCOPE_KEY, cancellationToken: cancellationToken);
            var keyFacts = await context.ReadStateAsync<List<string>>(WorkflowConstants.WORKFLOW_CONTEXT_KEY_FACTS_KEY, scopeName: WorkflowConstants.WORKFLOW_DEFAULT_SCOPE_KEY, cancellationToken: cancellationToken);

            // Construct the messages for the agent
            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, string.Format(USER_REQUEST_TEMPLATE, originalRequestByUser)),
            };

            if (keyFacts.Any())
            {
                messages.Add(new ChatMessage(ChatRole.User, string.Format(KEY_FACTS_TEMPLATE, string.Join(", ", keyFacts))));
            }

            if (input.IsError)
            {
                messages.Add(new ChatMessage(ChatRole.User, $"The code execution produced an error: {input.ExecutionResult}"));
            }
            else
            {
                messages.Add(new ChatMessage(ChatRole.User, $"The code execution produced this result: {input.ExecutionResult}"));
            }


            // Invoke the agent
            var response = await this._agent.RunAsync(messages, cancellationToken: cancellationToken);

            return response.Text;
        }


        [MessageHandler]
        public async ValueTask<string> HandleAsync(BusinessAdvisorAgentOutput input, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            var originalRequestByUser = await context.ReadStateAsync<string>(WorkflowConstants.WORKFLOW_ORIGINAL_REQUEST_BY_USER_KEY, scopeName: WorkflowConstants.WORKFLOW_DEFAULT_SCOPE_KEY, cancellationToken: cancellationToken);
            var keyFacts = await context.ReadStateAsync<List<string>>(WorkflowConstants.WORKFLOW_CONTEXT_KEY_FACTS_KEY, scopeName: WorkflowConstants.WORKFLOW_DEFAULT_SCOPE_KEY, cancellationToken: cancellationToken);

            // Construct the messages for the agent
            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, string.Format(USER_REQUEST_TEMPLATE, originalRequestByUser)),
            };

            if (keyFacts.Any())
            {
                messages.Add(new ChatMessage(ChatRole.User, string.Format(KEY_FACTS_TEMPLATE, string.Join(", ", keyFacts))));
            }

            messages.Add(new ChatMessage(ChatRole.User, $"Use the following documentation as a reference:\n{input.Documentation}"));


            // Invoke the agent
            var response = await this._agent.RunAsync(messages, cancellationToken: cancellationToken);

            return response.Text;
        }

        protected override RouteBuilder ConfigureRoutes(RouteBuilder routeBuilder)
        {
            return routeBuilder
                .AddHandler<RouterAgentOutput, string>((input, context) => HandleAsync(input, context, CancellationToken.None))
                .AddHandler<CodeSandboxOutput, string>((input, context) => HandleAsync(input, context, CancellationToken.None))
                .AddHandler<BusinessAdvisorAgentOutput, string>((input, context) => HandleAsync(input, context, CancellationToken.None));
        }
    }
}
