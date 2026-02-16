using AgentFrameworkCodeMode.Models.StructuredOutputs;
using AgentFrameworkCodeMode.Skills;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgentFrameworkCodeMode.Executors
{
    internal class CoderExecutor : Executor<BusinessAnalystAgentOutput, CoderAgentOutput>
    {
        private const string AGENT_NAME = "Coder";

        private readonly AIAgent _agent;
        private readonly ISkillProvider _skillProvider;


        public CoderExecutor(AIAgent agent, ISkillProvider skillProvider) : base("Coder")
        {
            _agent = agent;
            _skillProvider = skillProvider;
        }

        public override async ValueTask<CoderAgentOutput> HandleAsync(BusinessAnalystAgentOutput input, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            // fixed per ora
            var skill = "Statistics";
            var skillDetails = await _skillProvider.GetSkillAsync(skill, AGENT_NAME, cancellationToken);

            var messages = new List<ChatMessage>();
            messages.Add(new ChatMessage(ChatRole.System, $"API reference:\n{skillDetails}"));

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
