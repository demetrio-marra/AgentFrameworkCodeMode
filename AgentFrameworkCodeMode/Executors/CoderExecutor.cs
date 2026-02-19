using AgentFrameworkCodeMode.Models.Sandbox;
using AgentFrameworkCodeMode.Models.StructuredOutputs;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text;

namespace AgentFrameworkCodeMode.Executors
{
    internal class CoderExecutor : Executor<BusinessAnalystAgentOutput, CoderAgentOutput>
    {
        private const string AGENT_NAME = "Coder";

        private readonly AIAgent _agent;
        private readonly ISandboxDocumentationProvider _documentationProvider;


        public CoderExecutor(AIAgent agent, ISandboxDocumentationProvider documentationProvider) : base("Coder")
        {
            _agent = agent;
            _documentationProvider = documentationProvider;
        }

        public override async ValueTask<CoderAgentOutput> HandleAsync(BusinessAnalystAgentOutput input, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            var messages = new List<ChatMessage>();
            messages.Add(new ChatMessage(ChatRole.System, $"Today date is {DateTime.UtcNow:d}"));

            var docs = await _documentationProvider.GetDocumentationAsync(input.FunctionsList, cancellationToken);
            if (docs.Any())
            {
                var skillDetails = string.Join("\n\n", docs.Select(d => $"Documentation for function: {d.Key}\n{d.Value}"));
                messages.Add(new ChatMessage(ChatRole.System, $"API reference:\n{skillDetails}"));
            }

            messages.Add(new ChatMessage(ChatRole.User, $"Implement the program following those requirements:\n{input.ProgramSpecification}"));

            // Invoke the agent
            var response = await this._agent.RunAsync(messages, cancellationToken: cancellationToken);

            var code = response.Text;
            // remove triple backticks and javascript if present and triple backticks closing
            if (code.StartsWith("```javascript"))
            {
                code = code.Replace("```javascript", "");
            }
            if (code.EndsWith("```"))
            {
                code = code.Substring(0, code.Length - 3);
            }


            var ret = new CoderAgentOutput
            {
                Code = code
            };

            return ret!;
        }
    }
}
