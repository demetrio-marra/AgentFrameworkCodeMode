using AgentFrameworkCodeMode.Configuration;
using AgentFrameworkCodeMode.Infrastructure.Embedding;
using AgentFrameworkCodeMode.Infrastructure.Sandbox;
using AgentFrameworkCodeMode.Infrastructure.Skills;
using AgentFrameworkCodeMode.Models;
using AgentFrameworkCodeMode.Models.Embedding;
using AgentFrameworkCodeMode.Models.Sandbox;
using AgentFrameworkCodeMode.Models.Skills;
using AgentFrameworkCodeMode.Skills;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AgentFrameworkCodeMode
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var builder = CreateHostBuilder(args);
            var host = builder.Build();
            host.Run();
        }

        static HostApplicationBuilder CreateHostBuilder(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            // Build configuration
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                              ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                              ?? "Production";

            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            // Configure logging from appsettings.json
            builder.Logging.ClearProviders();
            builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
            builder.Logging.AddConsole();

            // open telemetry
            var enableOpenTelemetry = builder.Configuration.GetValue<bool>("EnableOpenTelemetry");
            if (enableOpenTelemetry)
            {
                builder.Services.AddOpenTelemetry()
                .WithTracing(tracing =>
                {
                    tracing
                        // MUST match your agent sourceName
                        .AddSource("Agent-*")

                        .SetResourceBuilder(
                            ResourceBuilder.CreateDefault()
                                .AddService("MyService"))

                        .AddConsoleExporter(); // 👈 prints spans to console
                });
            }

            // Bind configuration sections to POCOs

            // Skills finder
            var qdrantConfiguration = new QDrantConfiguration();
            builder.Configuration.GetSection("QDrant").Bind(qdrantConfiguration);
            builder.Services.AddSingleton(qdrantConfiguration);
            builder.Services.AddSingleton<ISkillsFinder, QDrantSkillsFinder>();

            // Embedding configuration and service registration
            var embeddingConfiguration = new EmbeddingConfiguration();
            builder.Configuration.GetSection("Embedding").Bind(embeddingConfiguration);
            builder.Services.AddSingleton(embeddingConfiguration);
            builder.Services.AddHttpClient<IEmbeddingService, EmbeddingClient>();

            var sesJsSandboxConfig = new SESJSSandboxConfiguration();
            builder.Configuration.GetSection("SESJSSandbox").Bind(sesJsSandboxConfig);
            builder.Services.AddSingleton(sesJsSandboxConfig);

            builder.Services.AddSingleton<ISandbox, SESJSSandbox>();

            // Bind InferenceProviders configuration as dictionary
            var inferenceProvidersConfig = new Dictionary<string, InferenceProviderConfiguration>();
            builder.Configuration.GetSection("InferenceProviders").Bind(inferenceProvidersConfig);

            // Bind LLMs configuration as dictionary
            var llmsConfig = new Dictionary<string, LLMConfiguration>();
            builder.Configuration.GetSection("LLMs").Bind(llmsConfig);

            // Bind raw Agents configuration as dictionary
            var rawAgentsConfig = new Dictionary<string, Configuration.AgentConfiguration>();
            builder.Configuration.GetSection("Agents").Bind(rawAgentsConfig);

            // Build complete agent configurations with resolved LLM and InferenceProvider data
            var agentBuilder = new AgentConfigurationBuilder(rawAgentsConfig, llmsConfig, inferenceProvidersConfig);
            var agentsConfig = agentBuilder.Build();
            var agentsConfigModels = agentsConfig.ToDictionary(kvp => kvp.Key, kvp => new Models.AgentConfiguration
            {
                ModelTemperature = kvp.Value.ModelTemperature,
                SystemPromptFile = kvp.Value.SystemPromptFile,
                CostPerMillionInputTokens = kvp.Value.CostPerMillionInputTokens,
                CostPerMillionOutputTokens = kvp.Value.CostPerMillionOutputTokens,
                Model = kvp.Value.Model,
                Provider = kvp.Value.Provider,
                Endpoint = kvp.Value.Endpoint,
                ApiKey = kvp.Value.ApiKey,
                UseStructuredOutput = kvp.Value.UseStructuredOutput,
                StructuredOutputFQCN = kvp.Value.StructuredOutputFQCN,
                StructuredOutputDescription = kvp.Value.StructuredOutputDescription
            });
            builder.Services.AddSingleton(agentsConfigModels);

            // Register SkillProvider as singleton
            builder.Services.AddSingleton<ISkillProvider, SkillProvider>();

            // Register AgentFactory as singleton
            builder.Services.AddSingleton<IAgentFactory, AgentFactory>();

            // Register MainService as singleton
            builder.Services.AddHostedService<ConsoleMainLoopService>();

            return builder;
        }
    }
}
