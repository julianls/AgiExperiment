using AgiExperiment.AI.Domain.Data.Model;
using AgiExperiment.AI.Cortex.Settings;
using AgiExperiment.AI.Cortex.Settings.PluginSelector;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using AgiExperiment.AI.Cortex.Common;
using ModelContextProtocol.Client;
using AgiExperiment.AI.Cortex.Settings.McpSelector;
using Microsoft.IdentityModel.Tokens;

namespace AgiExperiment.AI.Cortex.Pipeline.Interceptors;

public class FunctionCallingInterceptor : InterceptorBase, IInterceptor
{
    private CancellationToken _cancellationToken;
    private KernelService _kernelService;
    private readonly ILocalStorageService? _localStorageService;
    private readonly PluginsRepository _pluginsRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly ModelConfigurationService _modelConfigurationService;
    protected readonly McpClient _mcpClient;

    public FunctionCallingInterceptor(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _kernelService = _serviceProvider.GetRequiredService<KernelService>();
        _localStorageService = _serviceProvider.GetRequiredService<ILocalStorageService>();
        _pluginsRepository = _serviceProvider.GetRequiredService<PluginsRepository>();
        _modelConfigurationService = _serviceProvider.GetRequiredService<ModelConfigurationService>();
        _mcpClient = _serviceProvider.GetService<McpClient>();
    }

    public override string Name { get; } = "Function calling (select plugins)";


    public override async Task<Conversation> Send(Kernel kernel, Conversation conversation,
        Func<string, Task<string>>? onComplete = null,
        CancellationToken cancellationToken = default)
    {
        _cancellationToken = cancellationToken;
  
        await Intercept(kernel, conversation, cancellationToken);
        conversation.StopRequested = true;
      
        return conversation;
    }
 

    private async Task Intercept(Kernel kernel, Conversation conversation, CancellationToken cancellationToken)
    {
        var conversationState = _serviceProvider.GetRequiredService<CurrentConversationState>();
        conversationState.SetCurrentConversationForUser(conversation);

        var functionFilter = _serviceProvider.GetRequiredService<FunctionCallingFilter>();

        var approvalFilter = _serviceProvider.GetRequiredService<FunctionApprovalFilter>();

        var config = await _modelConfigurationService.GetConfig();
       
        kernel  = await _kernelService.CreateKernelAsync(config.Provider, config.Model, functionInvocationFilters: [functionFilter, approvalFilter]);

        await LoadPluginsAsync(kernel);
        await LoadMCPsAsync(kernel);

        conversation.AddMessage("assistant", "");
        OnUpdate?.Invoke();

        ChatHistory chatHistory = conversation.ToChatHistory();

        try
        {
            var promptExecutionSettings = _kernelService.GetPromptExecutionSettings(config.Provider, _modelConfigurationService.GetDefaultConfig().MaxTokens);

            IChatCompletionService chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
       
            var response = await chatCompletion.GetChatMessageContentAsync(
                chatHistory,
                executionSettings: promptExecutionSettings,
                kernel: kernel, cancellationToken: cancellationToken);

            conversation.Messages.Last().Content = response.Content;
        }
        catch (Exception e)
        {
            conversation.Messages.Last().Content = e.Message;
        }
        finally
        {
            OnUpdate?.Invoke();

            conversationState.RemoveCurrentConversation(conversation.UserId);
        }
    }

    private async Task LoadMCPsAsync(Kernel kernel)
    {
        var mcpClients = new List<(string Name, McpClient Client)>();

        try
        {
            if (_localStorageService != null)
            {
                var mcpSelections = await _localStorageService.GetItemAsync<List<McpSelection>>(Constants.McpServersKey, _cancellationToken);
                var selected = mcpSelections?.Where(s => s.Selected).Select(s => s.Name).ToHashSet(StringComparer.OrdinalIgnoreCase)
                              ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                if (selected.Contains("Playwright"))
                {
                    mcpClients.Add(("Playwright", await GetMCPClientForPlaywright()));
                }

                if (selected.Contains("GitHub"))
                {
                    mcpClients.Add(("GitHub", await GetMCPClientForGithub()));
                }

                if (selected.Contains("AspNetCoreSse") && _mcpClient != null)
                {
                    mcpClients.Add(("AspNetCoreSse", _mcpClient));
                }
            }
        }
        catch
        {
            // ignore MCP selection read errors
        }

        foreach (var entry in mcpClients)
        {
            try
            {
                var tools = await entry.Client.ListToolsAsync().ConfigureAwait(false);
                kernel.Plugins.AddFromFunctions(entry.Name,
                    tools.Select(aiFunction => aiFunction.AsKernelFunction()));
            }
            catch
            {
                // ignore individual MCP load errors
            }
        }
    }

    private async Task LoadPluginsAsync(Kernel kernel)
    {
        var semanticPlugins =  _pluginsRepository.GetSemanticPlugins();

        List<Plugin> pluginsEnabledInSettings = new List<Plugin>();
        IEnumerable<string> enabledNames = Enumerable.Empty<string>();
        if (_localStorageService != null)
        {
            pluginsEnabledInSettings =
                await _localStorageService.GetItemAsync<List<Plugin>>(Constants.PluginsKey, _cancellationToken);
            if(!pluginsEnabledInSettings.IsNullOrEmpty())
            {
                enabledNames = pluginsEnabledInSettings.Select(o => o.Name);
                semanticPlugins = semanticPlugins.Where(o => enabledNames.Contains(o.Name)).ToList();
            }
        }

        foreach (var plugin in semanticPlugins)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", plugin.Name);
            kernel.ImportPluginFromPromptDirectory(path, plugin.Name);
        }

        var nativePlugins = new List<Plugin>();
        nativePlugins.AddRange(_pluginsRepository.GetCoreNative());
        nativePlugins.AddRange(_pluginsRepository.GetExternalNative());
        nativePlugins.AddRange(_pluginsRepository.GetSemanticKernelPlugins());
        nativePlugins.AddRange(_pluginsRepository.GetKernelMemoryPlugins());
        var bing = _pluginsRepository.CreateBingPlugin();
        if (bing != null) nativePlugins.Add(bing);
        var google = _pluginsRepository.CreateGooglePlugin();
        if (google != null) nativePlugins.Add(google);

        nativePlugins = nativePlugins.Where(o => enabledNames.Contains(o.Name)).ToList();

        foreach (var plugin in nativePlugins)
        {
            try
            {
                string pluginName = plugin.Name.Substring(plugin.Name.LastIndexOf(".", StringComparison.Ordinal) + 1);
                kernel.ImportPluginFromObject(plugin.Instance, pluginName);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Could not load native plugins", e);
            }
        }
    }

    public static async Task<McpClient> GetMCPClientForPlaywright()
    {
        McpClientOptions options = new()
        {
            ClientInfo = new() { Name = "Playwright", Version = "1.0.0" }
        };

        var config = new StdioClientTransport(new()
        {
            Name = "Playwright",
            Command = "npx",
            Arguments = ["-y", "@playwright/mcp@latest"],
        });

        var mcpClient = await McpClient.CreateAsync(
            config,
            options
            );

        return mcpClient;
    }

    public static async Task<McpClient> GetMCPClientForGithub()
    {
        // Create an MCPClient for the GitHub server
        var mcpClient = await McpClient.CreateAsync(new StdioClientTransport(new()
        {
            Name = "GitHub",
            Command = "npx",
            Arguments = ["-y", "@modelcontextprotocol/server-github"],
        })).ConfigureAwait(false);

        return mcpClient;
    }
}