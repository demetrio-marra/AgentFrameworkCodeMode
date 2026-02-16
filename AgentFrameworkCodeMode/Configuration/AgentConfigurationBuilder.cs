using System.Collections.Generic;

namespace AgentFrameworkCodeMode.Configuration
{
    public class AgentConfigurationBuilder
    {
        private readonly Dictionary<string, AgentConfiguration> _rawAgents;
        private readonly Dictionary<string, LLMConfiguration> _llms;
        private readonly Dictionary<string, InferenceProviderConfiguration> _inferenceProviders;

        public AgentConfigurationBuilder(
            Dictionary<string, AgentConfiguration> rawAgents,
            Dictionary<string, LLMConfiguration> llms,
            Dictionary<string, InferenceProviderConfiguration> inferenceProviders)
        {
            _rawAgents = rawAgents;
            _llms = llms;
            _inferenceProviders = inferenceProviders;
        }

        public Dictionary<string, AgentConfiguration> Build()
        {
            var result = new Dictionary<string, AgentConfiguration>();

            foreach (var (agentName, agentConfig) in _rawAgents)
            {
                var fullConfig = new AgentConfiguration
                {
                    // Copy agent-specific properties
                    LLM = agentConfig.LLM,
                    ModelTemperature = agentConfig.ModelTemperature,
                    SystemPromptFile = agentConfig.SystemPromptFile,
                    UseStructuredOutput = agentConfig.UseStructuredOutput,
                    StructuredOutputFQCN = agentConfig.StructuredOutputFQCN,
                    StructuredOutputDescription = agentConfig.StructuredOutputDescription
                };

                // Resolve LLM configuration
                if (!string.IsNullOrEmpty(agentConfig.LLM) && _llms.TryGetValue(agentConfig.LLM, out var llmConfig))
                {
                    fullConfig.CostPerMillionInputTokens = llmConfig.CostPerMillionInputTokens;
                    fullConfig.CostPerMillionOutputTokens = llmConfig.CostPerMillionOutputTokens;
                    fullConfig.Model = llmConfig.Model;
                    fullConfig.Provider = llmConfig.Provider;

                    // Resolve InferenceProvider configuration
                    if (!string.IsNullOrEmpty(llmConfig.Provider) && 
                        _inferenceProviders.TryGetValue(llmConfig.Provider, out var providerConfig))
                    {
                        fullConfig.Endpoint = providerConfig.Endpoint;
                        fullConfig.ApiKey = providerConfig.ApiKey;
                    }
                }

                result[agentName] = fullConfig;
            }

            return result;
        }
    }
}
