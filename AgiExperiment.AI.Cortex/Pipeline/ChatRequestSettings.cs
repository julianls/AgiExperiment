using Microsoft.SemanticKernel;

namespace AgiExperiment.AI.Cortex.Pipeline;

public class ChatRequestSettings : PromptExecutionSettings
{
    public ChatRequestSettings()
    {
        ExtensionData = new Dictionary<string, object>
        {
            
        };
    }
}