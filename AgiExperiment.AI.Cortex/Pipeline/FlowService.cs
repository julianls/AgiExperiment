using AgiExperiment.AI.Cortex.Settings;
using AgiExperiment.AI.Domain.Data;
using AgiExperiment.AI.Domain.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace AgiExperiment.AI.Cortex.Pipeline
{
    public class FlowService
    {
        private IServiceProvider _serviceProvider;
        private KernelService _kernelService;
        private PipelineOptions _pipelineOptions;
        private ConversationsRepository _conversationsRepository;
        public IDbContextFactory<AiExperimentDBContext> _dbContextFactory;
        private ILogger<FlowService> _logger;

        public FlowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _kernelService = serviceProvider.GetService<KernelService>();
            _conversationsRepository = serviceProvider.GetService<ConversationsRepository>();
            _dbContextFactory = serviceProvider.GetService<IDbContextFactory<AiExperimentDBContext>>();
            _pipelineOptions = serviceProvider.GetRequiredService<IOptions<PipelineOptions>>().Value;
            _logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<FlowService>();
        }

        public async Task<Conversation> RunFlowAsync(Diagram diagram, Func<string, 
            Task<string>>? onStreamCompletion = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Running flow for diagram {diagramId}", diagram.Id);

            var modelConfiguration = null as ModelConfiguration;

            var conversation = new Conversation
            {
                Summary = diagram.Name + diagram.Id.ToString(),
                Model = !string.IsNullOrEmpty(modelConfiguration?.Model) ? modelConfiguration!.Model : _pipelineOptions.Providers.GitHub.ChatModel!,
                UserId = diagram.UserId
            };

            var startDiagramNode = diagram.DiagramNodes.Where(n => n.DiagramNodePorts.Count == 1).FirstOrDefault();

            var flowNodes = GetNodesFromStart(startDiagramNode, diagram);
            
            // Add system message
            var userMessage = new ConversationMessage("system", startDiagramNode.Message);
            conversation.AddMessage(userMessage);

            // Run the flow
            foreach (var item in flowNodes.Skip(1))
            {
                await RunFlowStepAsync(item, conversation, onStreamCompletion, cancellationToken);
            }

            _logger.LogInformation("Completed flow for diagram {diagramId}", diagram.Id);

            return conversation;
        }

        private async Task<Conversation> RunFlowStepAsync(DiagramNode diagramNode, Conversation conversation, 
            Func<string, Task<string>>? onStreamCompletion = null,
            CancellationToken cancellationToken = default)
        {

            var prompt = diagramNode.Message?.TrimEnd('\n');
            var rerun = false;

            if (!conversation.HasStarted())
            {
                var selected = /*_profileSelectorStart != null ? _profileSelectorStart.SelectedProfiles :*/ new List<QuickProfile>();

                string startMsg = string.Join(" ", selected.Select(p => p.Content));
                if (!string.IsNullOrEmpty(startMsg))
                    startMsg += "\n\n";

                if (!rerun)
                {
                    var userMessage = new ConversationMessage("user", startMsg + prompt!);
                    //foreach (var item in attachments)
                    //{
                    //    userMessage.MessageAttachments.Add(item);
                    //}
                    conversation.AddMessage(userMessage);
                    conversation.DateStarted = DateTime.UtcNow;
                }

            }
            else if (!rerun)
            {
                var userMessage = new ConversationMessage("user", prompt!);
                //foreach (var item in attachments)
                //{
                //    userMessage.MessageAttachments.Add(item);
                //}
                conversation.AddMessage(userMessage);

            }
            
            var modelConfiguration = GetNodeConfig(diagramNode);
            await Send(conversation, modelConfiguration, onStreamCompletion, cancellationToken);

            return conversation;
        }

        public ModelConfiguration GetNodeConfig(DiagramNode diagramNode)
        {
            return new ModelConfiguration()
            {
                Provider = ChatModelsProvider.GitHub,
                Model = _pipelineOptions.Providers.GitHub.ChatModel,
                MaxTokens = _pipelineOptions.MaxTokens,
                MaxPlannerTokens = _pipelineOptions.MaxPlannerTokens,
                Temperature = 0.0f,
                EmbeddingsModel = _pipelineOptions.Providers.GitHub.EmbeddingsModel,
                EmbeddingsProvider = EmbeddingsModelProvider.GitHub
            };
        }

        private List<DiagramNode> GetNodesFromStart(DiagramNode? startDiagramNode, Diagram diagram, 
            Func<string, Task<string>>? onStreamCompletion = null,
            CancellationToken cancellationToken = default)
        {
            if (startDiagramNode == null)
            {
                throw new ArgumentNullException(nameof(startDiagramNode), "Start diagram node cannot be null");
            }

            var visitedNodes = new HashSet<Guid>();
            var nodesToVisit = new Queue<DiagramNode>();
            var resultNodes = new List<DiagramNode>();

            nodesToVisit.Enqueue(startDiagramNode);

            while (nodesToVisit.Count > 0)
            {
                var currentNode = nodesToVisit.Dequeue();

                if (!visitedNodes.Add(currentNode.Id))
                {
                    continue;
                }

                resultNodes.Add(currentNode);

                var outgoingLinks = currentNode.DiagramNodePorts
                    .SelectMany(port => port.SourceNodeLinks);

                foreach (var link in outgoingLinks)
                {
                    var targetNode = link.TargetNodePort?.DiagramNode;

                    if (targetNode != null && !visitedNodes.Contains(targetNode.Id))
                    {
                        nodesToVisit.Enqueue(targetNode);
                    }
                }
            }
            return resultNodes;
        }

        private async Task Send(Conversation conversation, ModelConfiguration modelConfiguration,
            Func<string, Task<string>>? onStreamCompletion = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!conversation.StopRequested)
                {
                    conversation.AddMessage("assistant", "");

                    var chatRequestSettings = new ChatRequestSettings();
                    chatRequestSettings.ExtensionData["max_tokens"] = modelConfiguration!.MaxTokens;
                    chatRequestSettings.ExtensionData["temperature"] = modelConfiguration!.Temperature;
                    var kernel = await _kernelService.CreateKernelAsync(provider: modelConfiguration.Provider, model: modelConfiguration!.Model);
                    conversation = await
                        _kernelService.ChatCompletionAsStreamAsync(kernel, conversation, chatRequestSettings, onStreamCompletion, cancellationToken: cancellationToken);

                }


                await using var ctx = await _dbContextFactory.CreateDbContextAsync();

                ctx.Attach(conversation);

                if (conversation.Summary == null)
                {
                    var last = conversation.Messages.First(m => m.Role == ConversationRole.User).Content;
                    conversation.Summary = last.Substring(0, last.Length >= 75 ? 75 : last.Length);
                }

                await ctx.SaveChangesAsync();

                //conversation =
                //    await InterceptorHandler.Receive(_kernel, conversation,
                //       enabledInterceptorNames: await LocalStorageService.GetItemAsync<List<string>>(Constants.InterceptorsKey));
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("The operation was cancelled");
                conversation.Messages.RemoveAt(conversation.Messages.Count - 1);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred. Please try again/later. {Message}", e.Message);
                conversation.Messages.RemoveAt(conversation.Messages.Count - 1);
            }
            finally
            {
            }
        }
    }
}
