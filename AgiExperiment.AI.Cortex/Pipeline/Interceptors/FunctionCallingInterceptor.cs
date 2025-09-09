using AgiExperiment.AI.Domain.Data.Model;
using AgiExperiment.AI.Cortex.Settings;
using AgiExperiment.AI.Cortex.Settings.PluginSelector;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using AgiExperiment.AI.Cortex.Common;
using ModelContextProtocol.Client;

namespace AgiExperiment.AI.Cortex.Pipeline.Interceptors;

public class FunctionCallingInterceptor : InterceptorBase, IInterceptor
{
    private CancellationToken _cancellationToken;
    private KernelService _kernelService;
    private readonly ILocalStorageService? _localStorageService;
    private readonly PluginsRepository _pluginsRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly ModelConfigurationService _modelConfigurationService;
    protected readonly IMcpClient _mcpClient;

    public FunctionCallingInterceptor(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _kernelService = _serviceProvider.GetRequiredService<KernelService>();
        _localStorageService = _serviceProvider.GetRequiredService<ILocalStorageService>();
        _pluginsRepository = _serviceProvider.GetRequiredService<PluginsRepository>();
        _modelConfigurationService = _serviceProvider.GetRequiredService<ModelConfigurationService>();
        _mcpClient = _serviceProvider.GetService<IMcpClient>();
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

    private async Task LoadPluginsAsync(Kernel kernel)
    {
        var semanticPlugins =  _pluginsRepository.GetSemanticPlugins();

        List<Plugin> pluginsEnabledInSettings = new List<Plugin>();
        IEnumerable<string> enabledNames = Enumerable.Empty<string>();
        if (_localStorageService != null)
        {
            pluginsEnabledInSettings = 
                await _localStorageService.GetItemAsync<List<Plugin>>(Constants.PluginsKey, _cancellationToken);
            enabledNames = pluginsEnabledInSettings.Select(o => o.Name);
            semanticPlugins = semanticPlugins.Where(o => enabledNames.Contains(o.Name)).ToList();
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

        //// kernel memory plugin
        //var kernelMemoryPlugins = await _pluginsRepository.GetKernelMemoryPlugins();
        //kernelMemoryPlugins = kernelMemoryPlugins.Where(o => enabledNames.Contains(o.Name)).ToList();

        //foreach (var plugin in kernelMemoryPlugins)
        //{
        //    try
        //    {
        //        string pluginName = plugin.Name.Substring(plugin.Name.LastIndexOf(".", StringComparison.Ordinal) + 1);
        //        kernel.ImportPluginFromObject(plugin.Instance, pluginName);
        //    }
        //    catch (Exception e)
        //    {
        //        throw new InvalidOperationException("Could not load kernel memory plugins", e);
        //    }
        //}

        var mcpClients = new List<IMcpClient>();
        mcpClients.Add(await GetMCPClientForPlaywright());
        //mcpClients.Add(await GetMCPClientForGithub());

        int idx = 0;
        foreach (var mcpClient in mcpClients)
        {
            // Retrieve the list of tools available on the MCP server
            var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
            kernel.Plugins.AddFromFunctions($"MCP{idx++}", tools.Select(aiFunction => aiFunction.AsKernelFunction()));
        }

        if(_mcpClient != null)
        {
            // Retrieve the list of tools available on the MyMCP server
            var tools = await _mcpClient.ListToolsAsync().ConfigureAwait(false);
            kernel.Plugins.AddFromFunctions($"MyMCP", tools.Select(aiFunction => aiFunction.AsKernelFunction()));
        }
    }

    public static async Task<IMcpClient> GetMCPClientForPlaywright()
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

        var mcpClient = await McpClientFactory.CreateAsync(
            config,
            options
            );

        return mcpClient;
    }

    public static async Task<IMcpClient> GetMCPClientForGithub()
    {
        // Create an MCPClient for the GitHub server
        var mcpClient = await McpClientFactory.CreateAsync(new StdioClientTransport(new()
        {
            Name = "GitHub",
            Command = "npx",
            Arguments = ["-y", "@modelcontextprotocol/server-github"],
        })).ConfigureAwait(false);

        return mcpClient;
    }
}