using AgentFrameworkCodeMode.Models.Sandbox;
using AgentFrameworkCodeMode.Models.StructuredOutputs;
using Microsoft.Agents.AI.Workflows;

namespace AgentFrameworkCodeMode.Executors
{
    internal class CodeSandboxExecutor : Executor<CoderAgentOutput, CodeSandboxOutput>
    {
        private readonly string _agentId;
        private readonly ISandbox _sandbox;


        public CodeSandboxExecutor(string agentId, ISandbox sandbox) : base("CodeSandbox")
        {
            _agentId = agentId ?? throw new ArgumentNullException(nameof(agentId));
            _sandbox = sandbox ?? throw new ArgumentNullException(nameof(sandbox));
        }

        public override async ValueTask<CodeSandboxOutput> HandleAsync(CoderAgentOutput input, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            var sandboxOutput = await _sandbox.ExecuteCodeAsync(_agentId, input.Code, cancellationToken);
            return new CodeSandboxOutput
            {
                IsError = sandboxOutput.IsError,
                ExecutionResult = sandboxOutput.ExecutionResult
            };
        }
    }
}
