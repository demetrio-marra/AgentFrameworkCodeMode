using AgentFrameworkCodeMode.Configuration;
using AgentFrameworkCodeMode.Executors;
using AgentFrameworkCodeMode.Infrastructure.Sandbox;
using AgentFrameworkCodeMode.Models;
using AgentFrameworkCodeMode.Models.Inputs;
using AgentFrameworkCodeMode.Models.Sandbox;
using AgentFrameworkCodeMode.Models.Skills;
using AgentFrameworkCodeMode.Models.StructuredOutputs;
using AgentFrameworkCodeMode.Skills;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AgentFrameworkCodeMode
{
    internal class ConsoleMainLoopService : BackgroundService
    {
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IAgentFactory _agentFactory;
        private readonly ILogger<ConsoleMainLoopService> _logger;
        private readonly ISkillProvider _skillProvider;
        private readonly ISkillsFinder _skillsFinder;
        private readonly ISandbox _sandbox;

        public ConsoleMainLoopService(
            IHostApplicationLifetime lifetime,
            IAgentFactory agentFactory,
            ILogger<ConsoleMainLoopService> logger,
            ISkillProvider skillProvider,
            ISandbox sandbox,
            ISkillsFinder skillsFinder)
        {
            _lifetime = lifetime;
            _agentFactory = agentFactory ?? throw new ArgumentNullException(nameof(agentFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _skillProvider = skillProvider ?? throw new ArgumentNullException(nameof(skillProvider));
            _skillsFinder = skillsFinder ?? throw new ArgumentNullException(nameof(skillsFinder));
            _sandbox = sandbox ?? throw new ArgumentNullException(nameof(sandbox));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Give host time to complete initialization logging
            await Task.Delay(1000);
            await MainLoop(stoppingToken);
        }

        private async Task MainLoop(CancellationToken stoppingToken)
        {
            AIAgent contextAnalyzerAgent;
            AIAgent routerAgent;
            AIAgent businessAdvisorAgent;
            AIAgent businessAnalystAgent;
            AIAgent personalAssistantAgent;
            AIAgent coderAgent;
         
            try
            {
                contextAnalyzerAgent = await _agentFactory.CreateAgentAsync("ContextAnalyzer");
                routerAgent = await _agentFactory.CreateAgentAsync("Router");
                businessAdvisorAgent = await _agentFactory.CreateAgentAsync("BusinessAdvisor");
                businessAnalystAgent = await _agentFactory.CreateAgentAsync("BusinessAnalyst");
                personalAssistantAgent = await _agentFactory.CreateAgentAsync("PersonalAssistant");
                coderAgent = await _agentFactory.CreateAgentAsync("Coder");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create agent");
                return;
            }

            var contextAnalyzer = new ContextAnalyzerExecutor(contextAnalyzerAgent);
            var router = new RouterExecutor(routerAgent);
            var businessAnalyst = new BusinessAnalystExecutor(businessAnalystAgent, _skillsFinder);
            var businessAdvisor = new BusinessAdvisorExecutor(businessAdvisorAgent, _skillsFinder);
            var personalAssistant = new PersonalAssistantExecutor(personalAssistantAgent);
            var coder = new CoderExecutor(coderAgent, _skillProvider);
            var sandbox = new CodeSandboxExecutor("123", _sandbox);

            WorkflowBuilder wfb = new WorkflowBuilder(contextAnalyzer);
            wfb.AddEdge(contextAnalyzer, router);
            wfb.AddEdge(businessAdvisor, personalAssistant);
            wfb.AddSwitch(router, d =>
                d.AddCase<RouterAgentOutput>(r => r.RequestSubject == RouterAgentOutput.RouterAgentRequestSubject.Documentation, [businessAdvisor])
                 .AddCase<RouterAgentOutput>(r => r.RequestSubject == RouterAgentOutput.RouterAgentRequestSubject.BusinessAnalyst, [businessAnalyst])
                 .AddCase<RouterAgentOutput>(r => r.RequestSubject == RouterAgentOutput.RouterAgentRequestSubject.PersonalAssistant, [personalAssistant])
                 .WithDefault([personalAssistant])
                );
            wfb.AddEdge(businessAnalyst, coder);
            wfb.AddEdge(coder, sandbox);
            wfb.AddEdge(sandbox, personalAssistant);
            wfb.AddFanInEdge([businessAdvisor, sandbox], personalAssistant);
            wfb.WithOutputFrom(personalAssistant);

            var currentChatHistory = new List<Microsoft.Extensions.AI.ChatMessage>();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Console.Write("> ");
                    var input = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(input))
                    {
                        continue;
                    }
                    else if (string.Compare(input, "/exit") == 0)
                    {
                        _lifetime.StopApplication();
                        return;
                    }
                    else
                    {
                        var workflowInput = new ContextAnalyzerInput
                        {
                            RequestByUser = input,
                            History = currentChatHistory
                        };

                        var workflow = wfb.Build();

                        // Streaming execution - get events as they happen
                        await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, workflowInput, cancellationToken: stoppingToken);
                        
                        // Must send the turn token to trigger the agents.
                        // The agents are wrapped as executors. When they receive messages,
                        // they will cache the messages and only start processing when they receive a TurnToken.
                        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

                        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
                        {
                            switch (evt)
                            {
                                case ExecutorInvokedEvent invoke:
                                    Console.WriteLine($"\n[Agent: {invoke.ExecutorId}]");
                                    break;

                                case AgentResponseUpdateEvent updateEvent:
                                    Console.WriteLine($"\n[Agent Response Update: {updateEvent.ExecutorId}] {JsonSerializer.Serialize(updateEvent.Data)}");
                                    break;

                                case ExecutorCompletedEvent complete:
                                    Console.WriteLine($"\n[Completed: {complete.ExecutorId}, data: {JsonSerializer.Serialize(complete.Data)}]");
                                    break;

                                case WorkflowOutputEvent output:
                                    Console.WriteLine($"\n[Workflow Complete] Data: {JsonSerializer.Serialize(output.Data)}");
                                    break;

                                case WorkflowErrorEvent error:
                                    Console.WriteLine($"\n[Error] {error.Exception?.Message ?? "Unknown error"}");
                                    _logger.LogError(error.Exception, "Workflow error");
                                    break;
                            }
                        }


                        Console.WriteLine();
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in main loop");
                }
            }

            await Task.CompletedTask;
        }
    }
}
