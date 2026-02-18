namespace AgentFrameworkCodeMode.Models.StructuredOutputs
{
    public class ContextAnalyzerAgentOutput
    {
        public IEnumerable<string> KeyFacts { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> ActionableRequirements { get; set; } = Enumerable.Empty<string>();
    }
}
