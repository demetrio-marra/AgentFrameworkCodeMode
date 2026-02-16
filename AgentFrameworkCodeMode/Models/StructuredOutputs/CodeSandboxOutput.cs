namespace AgentFrameworkCodeMode.Models.StructuredOutputs
{
    internal class CodeSandboxOutput
    {
        public bool IsError { get; set; }
        public string ExecutionResult { get; set; } = string.Empty;
    }
}
