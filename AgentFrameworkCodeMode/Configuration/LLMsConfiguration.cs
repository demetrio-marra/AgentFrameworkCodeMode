using System.Collections.Generic;

namespace AgentFrameworkCodeMode.Configuration
{
    public class LLMConfiguration
    {
        public string CostPerMillionInputTokens { get; set; } = string.Empty;
        public string CostPerMillionOutputTokens { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
    }

    public class LLMsConfiguration
    {
        public Dictionary<string, LLMConfiguration> LLMs { get; set; } = new();
    }
}
