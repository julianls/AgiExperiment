﻿using System.Text;
using AgiExperiment.AI.Domain.Data.Model;
//using Blazored.LocalStorage;
using AgiExperiment.AI.Cortex.Settings;
using AgiExperiment.AI.Cortex.Settings.PluginSelector;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Collections.Generic;
using AgiExperiment.AI.Cortex.Common;

namespace AgiExperiment.AI.Cortex.Pipeline.Interceptors;

public class BotFunctionCallingFilter(CurrentConversationState conversationState, IFunctionCallingUserProvider userProvider , UserStorageService userStorage) : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        var id = await userStorage.GetUserIdFromLocalStorage();
        var conversation = conversationState.GetCurrentConversation(id);

        if (conversation == null)
        {
            throw new InvalidOperationException("Conversation is null. This filter requires a conversation to be set.");
        }

        await next(context);

        StringBuilder sb = new StringBuilder();
        sb.Append($"\n\n##### {context.Function.PluginName} {context.Function.Name}\n\n");
        foreach (var arg in context.Arguments.Names)
        {
            sb.Append("* " + arg + " : " + context.Arguments[arg] + "\n");
        }
        sb.Append("  \n");
        sb.Append("Result:\n" + context.Result + "\n");

        var lastUserMessage = conversation.Messages.FindLast(o => o.Role == ConversationRole.User)!;

        if (!string.IsNullOrEmpty(lastUserMessage?.ActionLog))
        {
            lastUserMessage.ActionLog += "\n\n---" + sb.ToString();
        }
        else
        {
            lastUserMessage.ActionLog = sb.ToString();

        }
    }
}

public class BotFunctionsInterceptor : InterceptorBase, IInterceptor
{
    private CancellationToken _cancellationToken;
    private KernelService _kernelService;
    private readonly ILocalStorageService? _localStorageService;
    private readonly PluginsRepository _pluginsRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly ModelConfigurationService _modelConfigurationService;
    private readonly UserStorageService _userStorage;

    public BotFunctionsInterceptor(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _kernelService = _serviceProvider.GetRequiredService<KernelService>();
        _localStorageService = _serviceProvider.GetRequiredService<ILocalStorageService>();
        _pluginsRepository = _serviceProvider.GetRequiredService<PluginsRepository>();
        _modelConfigurationService = _serviceProvider.GetRequiredService<ModelConfigurationService>();

        _userStorage = _serviceProvider.GetRequiredService<UserStorageService>();
    }

    public override string Name { get; } = "BotFunctionsInterceptor";


    public override async Task<Conversation> Send(Kernel kernel, Conversation conversation,
        Func<string, Task<string>>? onComplete = null,
        CancellationToken cancellationToken = default)
    {
        _cancellationToken = cancellationToken;
  
        await Intercept(kernel, conversation, cancellationToken);
        conversation.StopRequested = true;
      
        return conversation;
    }
 

    private  async Task Intercept(Kernel kernel, Conversation conversation, CancellationToken cancellationToken)
    {
        var conversationState = _serviceProvider.GetRequiredService<CurrentConversationState>();
        conversationState.SetCurrentConversationForUser(conversation);

        var functionFilter = new BotFunctionCallingFilter(conversationState, new FunctionCallingUserConsoleProvider(), _userStorage );




        var config = await _modelConfigurationService.GetConfig();
       kernel  = await _kernelService.CreateKernelAsync(config.Provider, config.Model, functionInvocationFilters: new List<IFunctionInvocationFilter>(){functionFilter});


        await LoadPluginsAsync(kernel);

        var prompt = conversation.Messages.Last().Content;
        var lastMsg = new ConversationMessage("assistant", "");
        conversation.Messages.Add(lastMsg);
        OnUpdate?.Invoke();

        try
        {

            var promptExecutionSettings = _kernelService.GetPromptExecutionSettings(config.Provider, _modelConfigurationService.GetDefaultConfig().MaxTokens);
            
            //OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            //{
            //    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            //    TopP = 0,
            //    MaxTokens = _modelConfigurationService.GetDefaultConfig().MaxTokens,
            //    Temperature = 0
            //};

            if (promptExecutionSettings.ExtensionData == null)
            {
                promptExecutionSettings.ExtensionData = new Dictionary<string, object>();
            }

            promptExecutionSettings.ExtensionData["UserId"] = "BotUser";

            IChatCompletionService chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

            ChatHistory chatHistory = conversation.ToChatHistory();
       
            var response = await chatCompletion.GetChatMessageContentAsync(
                chatHistory,
                executionSettings: promptExecutionSettings,
                kernel: kernel, cancellationToken: cancellationToken);

            lastMsg.Content = response.Content;
        }
        catch (Exception e)
        {
            lastMsg.Content = e.Message + "\n";
            OnUpdate?.Invoke();
        }
        finally
        {
            conversationState.RemoveCurrentConversation(conversation.UserId);
        }
    }

    private async Task LoadPluginsAsync(Kernel kernel)
    {
        var semanticPlugins = _pluginsRepository.GetSemanticPlugins();

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
        nativePlugins.AddRange(  _pluginsRepository.GetCoreNative());
        nativePlugins.AddRange(_pluginsRepository.GetExternalNative());

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
}