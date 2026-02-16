namespace AgentFrameworkCodeMode.Configuration
{
    public class AgentConfiguration
    {
        // Agent-specific properties
        public string LLM { get; set; } = string.Empty;
        public string ModelTemperature { get; set; } = string.Empty;
        public string SystemPromptFile { get; set; } = string.Empty;
       
        // LLM properties
        public string CostPerMillionInputTokens { get; set; } = string.Empty;
        public string CostPerMillionOutputTokens { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;

        // InferenceProvider properties
        public string Endpoint { get; set; } = string.Empty;
        public string? ApiKey { get; set; }

        // Structured output properties
        public bool UseStructuredOutput { get; set; }
        public string? StructuredOutputFQCN { get; set; } = string.Empty;
        public string? StructuredOutputDescription { get; set; }
    }
}
