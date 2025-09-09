using AgiExperiment.AI.Cortex.Pipeline;
using AgiExperiment.AI.Domain.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;
using AgiExperiment.AI.Cortex.Settings;
using Microsoft.Extensions.Options;

namespace AgiExperiment.AI.Cortex.Plugins
{
    public class LlmPlugin(IServiceProvider serviceProvider)
    {
        [KernelFunction]
        [Description("Returns chat message response content based on a chat query")]
        [return: Description("A list of message content")]
        public async Task<ReturnStringList> GetChatResponse(
            [Description("The topic, story, event etc that users needs to know more about")]
            string chatMessage,
            [Description("Provider name from following list (GitHub, Ollama, XAi, Gemini, DeepSeek, OpenAI, Anthropic)")]
            string providerName,
            [Description("Model name to use for given provider. For GitHub provder can be (gpt-4o). For Ollama provder can be (llama3.2:3b). For XAi provder can be (grok-2-vision). For Gemini provder can be (gemini-1.5-pro). For DeepSeek provder can be (deepseek-chat). For OpenAI provder can be (gpt-4o-mini). For Anthropic provder can be (claude-3-5-sonnet-20241022)")]
            string modelName)
        {
            var list = new ReturnStringList();

            var kernelService = serviceProvider.GetRequiredService<KernelService>();
            var modelConfiguration = GetModelConfiguration(providerName, modelName);
            var chatRequestSettings = new ChatRequestSettings();
            chatRequestSettings.ExtensionData["max_tokens"] = modelConfiguration!.MaxTokens;
            chatRequestSettings.ExtensionData["temperature"] = modelConfiguration!.Temperature;
            var kernel = await kernelService.CreateKernelAsync(provider: modelConfiguration.Provider, model: modelConfiguration!.Model);
            var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

            var history = new ChatHistory();
            history.AddUserMessage(chatMessage);

            var response = await chatCompletion.GetChatMessageContentAsync(history);
            if (!string.IsNullOrEmpty(response.Content))
            {
                list.Add(response.Content);
            }

            return list;
        }

        public ModelConfiguration GetModelConfiguration(string providerName, string modelName)
        {
            var pipelineOptions = serviceProvider.GetRequiredService<IOptions<PipelineOptions>>().Value;

            if (!Enum.TryParse<ChatModelsProvider>(providerName, out var provider) ||
                string.IsNullOrEmpty(modelName))
            {
                provider = ChatModelsProvider.GitHub;
                modelName = pipelineOptions.Providers.GitHub.ChatModel;
            }

            return new ModelConfiguration()
            {
                Provider = provider,
                Model = modelName,
                MaxTokens = pipelineOptions.MaxTokens,
                MaxPlannerTokens = pipelineOptions.MaxPlannerTokens,
                Temperature = 0.0f,
                EmbeddingsModel = pipelineOptions.Providers.GitHub.EmbeddingsModel,
                EmbeddingsProvider = EmbeddingsModelProvider.GitHub
            };
        }

    }
}
