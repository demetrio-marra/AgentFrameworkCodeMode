using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;
using System.ClientModel;

namespace AgentFrameworkCodeMode.Models
{
    public class AgentFactory : IAgentFactory
    {
        private readonly Dictionary<string, AgentConfiguration> _agentsConfig;

        public AgentFactory(Dictionary<string, AgentConfiguration> agentsConfig)
        {
            _agentsConfig = agentsConfig ?? throw new ArgumentNullException(nameof(agentsConfig));
        }

        public async Task<AIAgent> CreateAgentAsync(string agentName)
        {
            if (string.IsNullOrWhiteSpace(agentName))
                throw new ArgumentException("Agent name cannot be null or empty.", nameof(agentName));

            if (!_agentsConfig.TryGetValue(agentName, out var config))
                throw new KeyNotFoundException($"Agent configuration for '{agentName}' not found.");

            var systemPrompt = await LoadSystemPromptAsync(config.SystemPromptFile);

            var openAIClient = new OpenAIClient(new ApiKeyCredential(config.ApiKey ?? string.Empty), new OpenAIClientOptions
            {
                Endpoint = new Uri(config.Endpoint)
            });

            //#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
            //            var chatClient = openAIClient.GetResponsesClient(config.Model);
            //#pragma warning restore OPENAI001
            var chatClient = openAIClient.GetChatClient(config.Model)
                .AsIChatClient()
                .AsBuilder()
                //.UseOpenTelemetry(sourceName: "MyApplication", configure: (cfg) => cfg.EnableSensitiveData = true)
                .Build();

            var chatOptions = new ChatOptions
            {
                Temperature = float.Parse(config.ModelTemperature, System.Globalization.CultureInfo.InvariantCulture),
                Instructions = systemPrompt,
                ResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat.Text,
                ModelId = config.Model
            };

            // schema definition
            if (config.UseStructuredOutput)
            {
                if (string.IsNullOrWhiteSpace(config.StructuredOutputFQCN))
                {
                    throw new ArgumentException("Structured output FQCN must be provided when UseStructuredOutput is true.", nameof(config.StructuredOutputFQCN));
                }

                var structuredOutputType = Type.GetType(config.StructuredOutputFQCN);
                if (structuredOutputType == null)
                {
                    throw new TypeLoadException($"Could not load type for structured output FQCN: {config.StructuredOutputFQCN}");
                }

                // use reflection to get the type from the FQCN provided in config
                var schema = AIJsonUtilities.CreateJsonSchema(structuredOutputType);

                chatOptions.ResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat.ForJsonSchema(
                    schema: schema,
                    schemaDescription: config.StructuredOutputDescription);
            }

            var agent = chatClient.AsAIAgent(new ChatClientAgentOptions
            {
                 ChatOptions = chatOptions
            }).AsBuilder()
            .UseOpenTelemetry(sourceName: $"Agent-{agentName}", configure: (cfg) =>
            {
                cfg.EnableSensitiveData = true;
            })
            .Build();

            return agent;
        }

        private async Task<string> LoadSystemPromptAsync(string promptFile)
        {
            if (string.IsNullOrWhiteSpace(promptFile))
                throw new ArgumentException("System prompt file path cannot be null or empty.", nameof(promptFile));

            if (!File.Exists(promptFile))
                throw new FileNotFoundException($"System prompt file not found: {promptFile}", promptFile);

            return await File.ReadAllTextAsync(promptFile);
        }
    }
}
