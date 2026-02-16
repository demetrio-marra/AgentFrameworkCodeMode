namespace AgentFrameworkCodeMode.Models
{
    public class AgentConfiguration
    {
        public string ModelTemperature { get; set; } = string.Empty;
        public string SystemPromptFile { get; set; } = string.Empty;
        public string CostPerMillionInputTokens { get; set; } = string.Empty;
        public string CostPerMillionOutputTokens { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string? ApiKey { get; set; }
        public bool UseStructuredOutput { get; set; }
        public string? StructuredOutputFQCN { get; set; } = string.Empty;
        public string? StructuredOutputDescription { get; set; }
    }
}
