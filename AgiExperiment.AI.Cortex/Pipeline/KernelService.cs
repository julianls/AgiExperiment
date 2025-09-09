using AgiExperiment.AI.Domain.Data;
using AgiExperiment.AI.Domain.Data.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Redis;
using Microsoft.SemanticKernel.Memory;
using OpenAI;
using StackExchange.Redis;
using System.ClientModel;
using System.Text;
using Anthropic.SDK;
using Microsoft.Extensions.AI;
using TextContent = Microsoft.SemanticKernel.TextContent;
using AgiExperiment.AI.Cortex.Extensions;

namespace AgiExperiment.AI.Cortex.Pipeline;

public class KernelService
{
    protected readonly PipelineOptions _options;

    public KernelService(IOptions<PipelineOptions> options)
    {
        _options = options.Value;
    }

    public async Task<Kernel> CreateKernelAsync()
    {
        return await CreateKernelAsync(null, null);
    }

    public async Task<Kernel> CreateKernelAsync(string model)
    {
        return await CreateKernelAsync(null, model);
    }

    public async Task<Kernel> CreateKernelAsync(ChatModelsProvider? provider,
        string? model = null, IEnumerable<IPromptRenderFilter>? promptRenderFilters = null,
        IEnumerable<IFunctionInvocationFilter>? functionInvocationFilters = null, ILoggerFactory? loggerFactory = null)
    {
        if (model == "") model = null;
        var builder = Kernel.CreateBuilder();

        if (loggerFactory != null)
        {
            builder.Services.AddSingleton(loggerFactory);
        }

        if (provider == null)
        {
            if (_options.Providers.OpenAI.IsConfigured())
            {
                provider = ChatModelsProvider.OpenAI;
            }
            else if (_options.Providers.AzureOpenAI.IsConfigured())
            {
                provider = ChatModelsProvider.AzureOpenAI;
            }
            else if (_options.Providers.Ollama.IsConfigured())
            {
                provider = ChatModelsProvider.Ollama;
            }
            else if (_options.Providers.XAi.IsConfigured())
            {
                provider = ChatModelsProvider.XAi;
            }
            else if (_options.Providers.Gemini.IsConfigured())
            {
                provider = ChatModelsProvider.Gemini;
            }
            else if (_options.Providers.GitHub.IsConfigured())
            {
                provider = ChatModelsProvider.GitHub;
            }
            else if (_options.Providers.DeepSeek.IsConfigured())
            {
                provider = ChatModelsProvider.DeepSeek;
            }
            else if (_options.Providers.Anthropic.IsConfigured())
            {
                provider = ChatModelsProvider.Anthropic;
            }
            else if (_options.Providers.Local.IsConfigured())
            {
                provider = ChatModelsProvider.Local;
            }

            if (provider == null)
            {
                throw new InvalidOperationException("No model provider is configured");
            }
        }

        if (provider == ChatModelsProvider.AzureOpenAI)
        {
            model ??= _options.Providers.AzureOpenAI.ChatModel;

            builder
            .AddAzureOpenAIChatCompletion(
                deploymentName: _options.Providers.AzureOpenAI.ChatModels.First(p => p.Value == model).Key,
                modelId: model,
                endpoint: _options.Providers.AzureOpenAI.Endpoint,
                apiKey: _options.Providers.AzureOpenAI.ApiKey
                )
            .AddAzureOpenAITextEmbeddingGeneration(
                deploymentName: _options.Providers.AzureOpenAI.EmbeddingsModels.First(p => p.Value == _options.Providers.AzureOpenAI.EmbeddingsModel).Key,
                modelId: _options.Providers.AzureOpenAI.EmbeddingsModel,
                endpoint: _options.Providers.AzureOpenAI.Endpoint,
                apiKey: _options.Providers.AzureOpenAI.ApiKey
                );
        }
        if (provider == ChatModelsProvider.OpenAI)

        {
            model ??= _options.Providers.OpenAI.ChatModel;
            builder
                .AddOpenAIChatCompletion(model, _options.Providers.OpenAI.ApiKey)
                .AddOpenAITextEmbeddingGeneration(_options.Providers.OpenAI.EmbeddingsModel, _options.Providers.OpenAI.ApiKey)
                .AddOpenAITextToImage(_options.Providers.OpenAI.ApiKey);
        }

        if (provider == ChatModelsProvider.Ollama)
        {
            model ??= _options.Providers.Ollama.ChatModel;
            var ollamaUrl = new Uri(_options.Providers.Ollama.BaseUrl);
            builder.AddOpenAIChatCompletion(modelId: model, endpoint: ollamaUrl, apiKey: "")
                .AddOllamaTextEmbeddingGeneration(_options.Providers.Ollama.EmbeddingsModel, endpoint: ollamaUrl);
        }

        if (provider == ChatModelsProvider.XAi)
        {
            model ??= _options.Providers.XAi.ChatModel;
            var xaiUrl = new Uri(_options.Providers.XAi.BaseUrl);
            builder.AddOpenAIChatCompletion(modelId: model, endpoint: xaiUrl, apiKey: _options.Providers.XAi.ApiKey);
        }

        if (provider == ChatModelsProvider.Gemini)
        {
            model ??= _options.Providers.Gemini.ChatModel;
            builder.AddGoogleAIGeminiChatCompletion(modelId: model, apiKey: _options.Providers.Gemini.ApiKey);
        }

        if (provider == ChatModelsProvider.GitHub)
        {
            model ??= _options.Providers.GitHub.ChatModel;
            var githibUrl = new Uri(_options.Providers.GitHub.BaseUrl);
            // create client
            var client = new OpenAIClient(new ApiKeyCredential(_options.Providers.GitHub.ApiKey), new OpenAIClientOptions { Endpoint = githibUrl });
            builder.AddOpenAIChatCompletion(model, client);
        }

        if (provider == ChatModelsProvider.DeepSeek)
        {
            model ??= _options.Providers.DeepSeek.ChatModel;
            var uri = new Uri(_options.Providers.DeepSeek.BaseUrl);
            // create client
            var client = new OpenAIClient(new ApiKeyCredential(_options.Providers.DeepSeek.ApiKey), new OpenAIClientOptions { Endpoint = uri });
            builder.AddOpenAIChatCompletion(model, client);
        }

        if (provider == ChatModelsProvider.Anthropic)
        {
            model ??= _options.Providers.Anthropic.ChatModel;

            var anthropicClient = new AnthropicClient(new APIAuthentication(_options.Providers.Anthropic.ApiKey));

            // Use Anthropic's chat service with Semantic Kernel
            var chatService = new ChatClientBuilder(anthropicClient.Messages)
                .ConfigureOptions(cfg => cfg.ModelId = model)
                .UseFunctionInvocation()
                .Build()
                .AsChatCompletionService();

            // Replace the default chat completion service with Anthropic's
            builder.Services.AddSingleton(chatService);
        }

        if (promptRenderFilters != null)
        {
            foreach (var filter in promptRenderFilters)
            {
                builder.Services.AddSingleton(filter);
            }
        }

        if (functionInvocationFilters != null)
        {
            foreach (var filter in functionInvocationFilters)
            {
                builder.Services.AddSingleton(filter);
            }
        }

        return builder.Build();
    }

    public async Task<ISemanticTextMemory> GetMemoryStore()
    {
        return await GetMemoryStore(null, null);
    }

    public async Task<ISemanticTextMemory> GetMemoryStore(EmbeddingsModelProvider? provider, string? model)
    {
        if (provider == null)
        {
            if (_options.Providers.OpenAI.IsConfigured())
            {
                provider = EmbeddingsModelProvider.OpenAI;
            }
            else if (_options.Providers.AzureOpenAI.IsConfigured())
            {
                provider = EmbeddingsModelProvider.AzureOpenAI;
            }
            else if (_options.Providers.Ollama.IsConfigured())
            {
                provider = EmbeddingsModelProvider.Ollama;
            }
            else if (_options.Providers.XAi.IsConfigured())
            {
                provider = EmbeddingsModelProvider.XAi;
            }
            else if (_options.Providers.Gemini.IsConfigured())
            {
                provider = EmbeddingsModelProvider.Gemini;
            }
            else if (_options.Providers.GitHub.IsConfigured())
            {
                provider = EmbeddingsModelProvider.GitHub;
            }
            else if (_options.Providers.DeepSeek.IsConfigured())
            {
                provider = EmbeddingsModelProvider.DeepSeek;
            }
            else if (_options.Providers.Anthropic.IsConfigured())
            {
                provider = EmbeddingsModelProvider.Anthropic;
            }
            else if (_options.Providers.Local.IsConfigured())
            {
                provider = EmbeddingsModelProvider.Local;
            }

            if (provider == null)
            {
                throw new InvalidOperationException("No embeddings model provider is configured");
            }
        }


        IMemoryStore memoryStore = null!;
        //if (_options.Embeddings.UseSqlite)
        //    memoryStore = await SqliteMemoryStore.ConnectAsync(_options.Embeddings.SqliteConnectionString);
        //if (_options.Embeddings.UseRedis)
        //{
        //    var redis = ConnectionMultiplexer.Connect(_options.Embeddings.RedisConfigurationString);
        //    var _db = redis.GetDatabase();

        //    // local use would indicate nomic-embed, so adjust vectors
        //    var vectorSize = provider == EmbeddingsModelProvider.Ollama ? 768 : 1536;
        //    memoryStore = new RedisMemoryStore(_db, vectorSize);
        //}

        if (provider == EmbeddingsModelProvider.AzureOpenAI)
        {
            var generation = new AzureOpenAITextEmbeddingGenerationService(
                model ?? _options.Providers.AzureOpenAI.EmbeddingsModel,
                _options.Providers.AzureOpenAI.Endpoint,
                _options.Providers.AzureOpenAI.ApiKey
            );

            var mem = new MemoryBuilder()
                .WithTextEmbeddingGeneration(generation)
                .WithMemoryStore(memoryStore)
                .Build();

            return mem;
        }

        if (provider == EmbeddingsModelProvider.OpenAI)
        {
            var mem = new MemoryBuilder()
                .WithOpenAITextEmbeddingGeneration(
                    modelId: model ?? _options.Providers.OpenAI.EmbeddingsModel, _options.Providers.OpenAI.ApiKey)
                .WithMemoryStore(memoryStore)
                .Build();
            return mem;
        }

        if (provider == EmbeddingsModelProvider.Ollama)
        {
            var generation = new OllamaTextEmbeddingGenerationService(model ?? _options.Providers.Ollama.EmbeddingsModel, new Uri(_options.Providers.Ollama.BaseUrl));
            var mem = new MemoryBuilder()
                .WithTextEmbeddingGeneration(generation)
                .WithMemoryStore(memoryStore)
                .Build();
            return mem;
        }

        if (provider == EmbeddingsModelProvider.GitHub)
        {
            model ??= _options.Providers.GitHub.EmbeddingsModel;
            var githibUrl = new Uri(_options.Providers.GitHub.BaseUrl);
            // create client
            var client = new OpenAIClient(new ApiKeyCredential(_options.Providers.GitHub.ApiKey), new OpenAIClientOptions { Endpoint = githibUrl });
            var generation = new OpenAITextEmbeddingGenerationService(model ?? _options.Providers.GitHub.EmbeddingsModel, client);
            var mem = new MemoryBuilder()
                .WithTextEmbeddingGeneration(generation)
                .WithMemoryStore(memoryStore)
                .Build();
            return mem;
        }

        if (provider == EmbeddingsModelProvider.DeepSeek)
        {
            model ??= _options.Providers.DeepSeek.EmbeddingsModel;
            var uri = new Uri(_options.Providers.DeepSeek.BaseUrl);
            // create client
            var client = new OpenAIClient(new ApiKeyCredential(_options.Providers.DeepSeek.ApiKey), new OpenAIClientOptions { Endpoint = uri });
            var generation = new OpenAITextEmbeddingGenerationService(model ?? _options.Providers.DeepSeek.EmbeddingsModel, client);
            var mem = new MemoryBuilder()
                .WithTextEmbeddingGeneration(generation)
                .WithMemoryStore(memoryStore)
                .Build();
            return mem;
        }

        //if (provider == EmbeddingsModelProvider.Anthropic)
        //{
        //    var mem = new MemoryBuilder()
        //        .WithAnthropicTextEmbeddingGeneration(
        //            modelId: model ?? _options.Providers.Anthropic.EmbeddingsModel, _options.Providers.Anthropic.ApiKey)
        //        .WithMemoryStore(memoryStore)
        //        .Build();
        //    return mem;
        //}

        return new MemoryBuilder()
            .WithMemoryStore(memoryStore)
            .Build();
    }

    public async Task<Conversation> ChatCompletionAsStreamAsync(Kernel kernel,
        ChatHistory chatHistory,
        PromptExecutionSettings? requestSettings = default,
        Func<string, Task<string>>? onStreamCompletion = null,
        CancellationToken cancellationToken = default)
    {
        requestSettings ??= new ChatRequestSettings();

        var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
        var fullMessage = string.Empty;

        await foreach (var completionResult in chatCompletion.GetStreamingChatMessageContentsAsync(chatHistory,
                           requestSettings, cancellationToken: cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            fullMessage += completionResult.Content;
            if (onStreamCompletion != null) await onStreamCompletion.Invoke(completionResult.Content);

        }

        chatHistory.AddMessage(AuthorRole.Assistant, fullMessage);
        return chatHistory.ToConversation();
    }

    public async Task<Conversation> ChatCompletionAsStreamAsync(Kernel kernel,
        Conversation conversation,
        PromptExecutionSettings? requestSettings = default,
        Func<string, Task<string>>? onStreamCompletion = null,
        CancellationToken cancellationToken = default)
    {
        requestSettings ??= new ChatRequestSettings();

        var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
        var fullMessage = string.Empty;
        var history = conversation.ToChatHistory();

        await foreach (var completionResult in chatCompletion.GetStreamingChatMessageContentsAsync(history,
                           requestSettings, cancellationToken: cancellationToken))
        {

            cancellationToken.ThrowIfCancellationRequested();
            fullMessage += completionResult.Content;
            if (onStreamCompletion != null) await onStreamCompletion.Invoke(completionResult.Content);

        }

        conversation.Messages.Last().Content = fullMessage;
        return conversation;
    }

    internal PromptExecutionSettings GetPromptExecutionSettings(ChatModelsProvider? provider, int maxTokens)
    {
        if (provider == ChatModelsProvider.Gemini)
        {
            return new GeminiPromptExecutionSettings
            {
                MaxTokens = maxTokens,
                ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions,
                TopP = 0,
                Temperature = 0
            };
        }

        //if (provider == ChatModelsProvider.Anthropic)
        //{
        //    return new AnthropicPromptExecutionSettings
        //    {
        //        MaxTokens = maxTokens,
        //        //ToolCallBehavior = AnthropicToolCallBehavior.AutoInvokeKernelFunctions,
        //        TopP = 0,
        //        Temperature = 0
        //    };
        //}

        return new OpenAIPromptExecutionSettings
        {
            MaxTokens = maxTokens,
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            TopP = 0,
            Temperature = 0
        };
    }
}

public static class ChatExtensions
{
    public static ChatHistory ToChatHistory(this Conversation conversation)
    {
        var chatHistory = new ChatHistory();
        foreach (var message in conversation.Messages.Where(c => !string.IsNullOrEmpty(c.Content.Trim())))
        {
            var role =
                message.Role == "system"
                    ? AuthorRole.System
                    : // if the role is system, set the role to system
                    message.Role == "user"
                        ? AuthorRole.User
                        : AuthorRole.Assistant;

            if(message.MessageAttachments.Any())
            {
                var contentItems = message.ToMessageContentItemCollection();

                // Add a user message with both the image and a question
                // about the image.
                chatHistory.AddUserMessage(contentItems);
            }
            else
            {
                chatHistory.AddMessage(role, message.Content);
            }
        }

        return chatHistory;
    }

    public static ChatMessageContentItemCollection ToMessageContentItemCollection(this ConversationMessage message)
    {
        ChatMessageContentItemCollection result = [new TextContent(message.Content)];

        foreach (var attachment in message.MessageAttachments)
        {
            KernelContent contentItem;
            
            if (attachment.IsTextContent)
            {
                contentItem = new TextContent(Encoding.Default.GetString(attachment.Content));
            }
            else if (attachment.IsImageContent)
            {
                contentItem = new ImageContent(attachment.Content, attachment.ContentType);
            }
            else if (attachment.IsAudioContent)
            {
                contentItem = new AudioContent(attachment.Content, attachment.ContentType);
            }
            else if(!TryParseTextContent(attachment, out contentItem))
            {
                contentItem = new Microsoft.SemanticKernel.BinaryContent(attachment.Content, attachment.ContentType);
            }

            result.Add(contentItem);
        }

        return result;
    }

    private static bool TryParseTextContent(MessageAttachment attachment, out KernelContent contentItem)
    {
        try
        {
            var result = attachment.ToText();
            contentItem = new TextContent(result);
            return true;
        }
        catch (Exception)
        {
        }

        contentItem = null;
        return false;
    }

    public static Conversation ToConversation(this ChatHistory chatHistory)
    {
        var conversation = new Conversation();
        foreach (var message in chatHistory)
        {
            var role =
                message.Role == AuthorRole.System
                    ? "system"
                    : // if the role is system, set the role to system
                    message.Role == AuthorRole.User
                        ? "user"
                        : "assistant";

            if (message.Items.Count > 1 || !(message.Items.First() is TextContent))
            {
                var conversationMessage = new ConversationMessage(role, message.Content);
                conversation.AddMessage(conversationMessage);

                foreach (var contentItem in message.Items.Skip(1))
                {
                    if (contentItem is ImageContent imageContent)
                    {
                        conversationMessage.MessageAttachments.Add(new MessageAttachment
                        {
                            ContentType = contentItem.MimeType,
                            Content = imageContent.Data?.ToArray() ?? Array.Empty<byte>()
                        });
                    }
                    else if (contentItem is TextContent textContent)
                    {
                        conversationMessage.MessageAttachments.Add(new MessageAttachment
                        {
                            ContentType = contentItem.MimeType,
                            Content = Encoding.Default.GetBytes(textContent.Text)
                        });
                    }
                    else if (contentItem is AudioContent audioContent)
                    {
                        conversationMessage.MessageAttachments.Add(new MessageAttachment
                        {
                            ContentType = contentItem.MimeType,
                            Content = audioContent.Data?.ToArray() ?? Array.Empty<byte>()
                        });
                    }
                    else if (contentItem is Microsoft.SemanticKernel.BinaryContent binaryContent)
                    {
                        conversationMessage.MessageAttachments.Add(new MessageAttachment
                        {
                            ContentType = contentItem.MimeType,
                            Content = binaryContent.Data?.ToArray() ?? Array.Empty<byte>()
                        });
                    }
                }
            }
            else
            {
                conversation.AddMessage(new ConversationMessage(role, message.Content));
            }
        }

        return conversation;
    }
}