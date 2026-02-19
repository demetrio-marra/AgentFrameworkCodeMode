namespace AgentFrameworkCodeMode.Models.StructuredOutputs
{
    internal class BusinessAnalystAgentOutput
    {
        public string ProgramSpecification { get; set; } = string.Empty;
        public List<string> FunctionsList { get; set; } = new List<string>();
    }
}
