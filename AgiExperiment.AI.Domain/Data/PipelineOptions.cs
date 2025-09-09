using System.Text.Json.Serialization;
 

namespace AgiExperiment.AI.Domain.Data;

public class ModelsProvidersOptions
{
    public OpenAIModelsOptions OpenAI { get; set; } = new OpenAIModelsOptions();
   
    public AzureOpenAIModelsOptions AzureOpenAI { get; set; } = new AzureOpenAIModelsOptions();
   
    public LocalModelsOptions Local { get; set; } = new LocalModelsOptions();

    public OllamaOptions Ollama { get; set; } = new OllamaOptions();

    public XAiOptions XAi { get; set; } = new XAiOptions();

    public GeminiOptions Gemini { get; set; } = new GeminiOptions();

    public GitHubOptions GitHub { get; set; } = new GitHubOptions();

    public DeepSeekOptions DeepSeek { get; set; } = new DeepSeekOptions();

    public AnthropicOptions Anthropic { get; set; } = new AnthropicOptions();

    public ChatModelsProvider GetChatModelsProvider()
    {
        if (OpenAI.IsConfigured())
        {
            return ChatModelsProvider.OpenAI;
        }
        else
        {
            if (AzureOpenAI.IsConfigured())
            {
                return ChatModelsProvider.AzureOpenAI;
            }
            if (Ollama.IsConfigured())
            {
                return ChatModelsProvider.Ollama;
            }
            if (XAi.IsConfigured())
            {
                return ChatModelsProvider.XAi;
            }
            if (Gemini.IsConfigured())
            {
                return ChatModelsProvider.Gemini;
            }
            if (GitHub.IsConfigured())
            {
                return ChatModelsProvider.GitHub;
            }
            if (DeepSeek.IsConfigured())
            {
                return ChatModelsProvider.DeepSeek;
            }
            if (Anthropic.IsConfigured())
            {
                return ChatModelsProvider.Anthropic;
            }
            if (Local.IsConfigured())
            {
                return ChatModelsProvider.Local;
            }
        }

        return ChatModelsProvider.Local; 
    }

    public string GetChatModel()
    {

        if (OpenAI.IsConfigured())
        {
            return OpenAI.ChatModel;
        }
        else
        {
            if (AzureOpenAI.IsConfigured())
            {
                return AzureOpenAI.ChatModel;
            }
            if (Ollama.IsConfigured())
            {
                return Ollama.ChatModel;
            }
            if (XAi.IsConfigured())
            {
                return XAi.ChatModel;
            }
            if (Gemini.IsConfigured())
            {
                return Gemini.ChatModel;
            }
            if (GitHub.IsConfigured())
            {
                return GitHub.ChatModel;
            }
            if (DeepSeek.IsConfigured())
            {
                return DeepSeek.ChatModel;
            }
            if (Anthropic.IsConfigured())
            {
                return Anthropic.ChatModel;
            }
            if (Local.IsConfigured())
            {
                return Local.LocalModelName;
            }
        }

        return string.Empty;
    }

    public string GetEmbeddingsModel()
    {
        if (OpenAI.IsConfigured())
        {
            return OpenAI.EmbeddingsModel;
        }

        if (AzureOpenAI.IsConfigured())
        {
            return AzureOpenAI.EmbeddingsModel;
        }

        if (Ollama.IsConfigured())
        {
            return Ollama.EmbeddingsModel;
        }

        if (XAi.IsConfigured())
        {
            return XAi.EmbeddingsModel;
        }

        if (Gemini.IsConfigured())
        {
            return Gemini.EmbeddingsModel;
        }

        if (GitHub.IsConfigured())
        {
            return GitHub.EmbeddingsModel;
        }

        if (DeepSeek.IsConfigured())
        {
            return DeepSeek.EmbeddingsModel;
        }

        if (Anthropic.IsConfigured())
        {
            return Anthropic.EmbeddingsModel;
        }

        if (Local.IsConfigured())
        {
            return Local.EmbeddingsModel;
        }

        return string.Empty;
    }

    public EmbeddingsModelProvider GetEmbeddingsModelProvider()
    {
        if (OpenAI.IsConfigured())
        {
            return EmbeddingsModelProvider.OpenAI;
        }

        if (AzureOpenAI.IsConfigured())
        {
            return EmbeddingsModelProvider.AzureOpenAI;
        }

        if (Ollama.IsConfigured())
        {
            return EmbeddingsModelProvider.Ollama;
        }

        if (XAi.IsConfigured())
        {
            return EmbeddingsModelProvider.XAi;
        }

        if (Gemini.IsConfigured())
        {
            return EmbeddingsModelProvider.Gemini;
        }

        if (GitHub.IsConfigured())
        {
            return EmbeddingsModelProvider.GitHub;
        }

        if (DeepSeek.IsConfigured())
        {
            return EmbeddingsModelProvider.DeepSeek;
        }

        if (Anthropic.IsConfigured())
        {
            return EmbeddingsModelProvider.Anthropic;
        }

        if (Local.IsConfigured())
        {
            return EmbeddingsModelProvider.Local;
        }

        return EmbeddingsModelProvider.Local;
    }
}


public class OllamaOptions
{
	public string BaseUrl { get; set; } = "";
    public string[] Models { get; set; } = Array.Empty<string>();
    public string ChatModel { get; set; } = default!;

    public string[] EmbeddingsModels { get; set; } = Array.Empty<string>();
    public string EmbeddingsModel { get; set; } = default!;

    public bool IsConfigured()
    {
        return !string.IsNullOrEmpty(BaseUrl);
    }
}

public class XAiOptions
{
    public string BaseUrl { get; set; } = "";
    public string ApiKey { get; set; } = string.Empty;
    public string[] Models { get; set; } = Array.Empty<string>();
    public string ChatModel { get; set; } = default!;

    public string[] EmbeddingsModels { get; set; } = Array.Empty<string>();
    public string EmbeddingsModel { get; set; } = default!;

    public bool IsConfigured()
    {
        return !string.IsNullOrEmpty(BaseUrl);
    }
}

public class GeminiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string[] Models { get; set; } = Array.Empty<string>();
    public string ChatModel { get; set; } = default!;

    public string[] EmbeddingsModels { get; set; } = Array.Empty<string>();
    public string EmbeddingsModel { get; set; } = default!;

    public bool IsConfigured()
    {
        return !string.IsNullOrEmpty(ApiKey);
    }
}

public class GitHubOptions
{
    public string BaseUrl { get; set; } = "";
    public string ApiKey { get; set; } = string.Empty;
    public string[] Models { get; set; } = Array.Empty<string>();
    public string ChatModel { get; set; } = default!;

    public string[] EmbeddingsModels { get; set; } = Array.Empty<string>();
    public string EmbeddingsModel { get; set; } = default!;

    public bool IsConfigured()
    {
        return !string.IsNullOrEmpty(ApiKey);
    }
}

public class DeepSeekOptions
{
    public string BaseUrl { get; set; } = "";
    public string ApiKey { get; set; } = string.Empty;
    public string[] Models { get; set; } = Array.Empty<string>();
    public string ChatModel { get; set; } = default!;

    public string[] EmbeddingsModels { get; set; } = Array.Empty<string>();
    public string EmbeddingsModel { get; set; } = default!;

    public bool IsConfigured()
    {
        return !string.IsNullOrEmpty(ApiKey);
    }
}

public class AnthropicOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string[] Models { get; set; } = Array.Empty<string>();
    public string ChatModel { get; set; } = default!;

    public string[] EmbeddingsModels { get; set; } = Array.Empty<string>();
    public string EmbeddingsModel { get; set; } = default!;

    public bool IsConfigured()
    {
        return !string.IsNullOrEmpty(ApiKey);
    }
}

public class LocalModelsOptions
{
    public string LocalModelName { get; set; } = string.Empty;

    public string ChatModel { get; set; } = string.Empty;
    public string[] ChatModels { get; set; } = Array.Empty<string>();

    public string EmbeddingsModel { get; set; } = string.Empty;
    public string[] EmbeddingsModels { get; set; } = Array.Empty<string>();

    public bool IsConfigured()
    {
        return string.IsNullOrEmpty(LocalModelName);
    }
}

public enum ChatModelsProvider
{
    OpenAI,
    AzureOpenAI,
    Ollama,
    XAi,
    Gemini,
    GitHub,
    DeepSeek,
    Anthropic,
    Local
}

public enum EmbeddingsModelProvider
{
    OpenAI,
    AzureOpenAI,
    Ollama,
    XAi,
    Gemini,
    GitHub,
    DeepSeek,
    Anthropic,
    Local
}


public class OpenAIModelsOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string ChatModel { get; set; } = string.Empty;
    public string[] ChatModels { get; set; } = Array.Empty<string>();

    public string EmbeddingsModel { get; set; } = string.Empty;
    public string[] EmbeddingsModels { get; set; } = Array.Empty<string>();

    public bool IsConfigured()
    {
        return !string.IsNullOrEmpty(ApiKey);
    }
}

public class AzureOpenAIModelsOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;

    public string ChatModel { get; set; } = string.Empty;
    public Dictionary<string, string> ChatModels { get; set; } = new Dictionary<string, string>();

    public string EmbeddingsModel { get; set; } = string.Empty;


    // deploymentId, modelId
    public Dictionary<string,string> EmbeddingsModels { get; set; } = new Dictionary<string,string>();

    public bool IsConfigured()
    {
        return !string.IsNullOrEmpty(ApiKey);
    }
}

public class PipelineOptions
{
    public ModelsProvidersOptions Providers { get; set; } =  new ModelsProvidersOptions();

    public int MaxTokens { get; set; }
    public int MaxPlannerTokens { get; set; }

    public string[]? EnabledInterceptors { get; set; }
    public string[]? PreSelectedInterceptors { get; set; }

    public string? KrokiHost { get; set; } = default!;
    public string? StateFileSaveInterceptorPath { get; set; } = default!;

    public EmbeddingsSettings Embeddings { get; set; } = new EmbeddingsSettings();

    public string? BING_API_KEY { get; set; } = default!;
    public string? GOOGLE_API_KEY { get; set; } = default!;
    public string? GOOGLE_SEARCH_ENGINE_ID { get; set; } = default!;

    public Bot Bot { get; set; } = new Bot();

    public MemoryOptions Memory { get; set; } = new MemoryOptions();

}

public class MemoryOptions
{
    public bool Enabled { get; set; }
    public string Url { get; set; } = default!;
    public string ApiKey { get; set; } = default!;

    public string AzureStorageConnectionString { get; set; } = default!;
    // redis
    public string RedisConnectionString { get; set; } = default!;

}

public class Bot
{
    public string BotSystemInstruction { get; set; } = default!;
    public string BotUserId { get; set; } = default!;
}

public class EmbeddingsSettings
{
    public string RedisConfigurationString { get; set; } = default!;
    public string RedisIndexName { get; set; } = default!;
    public int MaxTokensToIncludeAsContext { get; set; } = default!;
    public bool UseRedis { get; set; }
    public bool UseSqlite { get; set; }
    public string SqliteConnectionString { get; set; } = default!;
}
