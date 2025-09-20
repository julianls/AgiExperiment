using AgiExperiment.AI.Cortex.Common;
using AgiExperiment.AI.Cortex.Settings.McpSelector;
using AgiExperiment.AI.Cortex.Pipeline.Interceptors;

namespace AgiExperiment.AI.Cortex.Settings;

public class McpConfigurationService(AgiExperiment.AI.Cortex.Pipeline.Interceptors.ILocalStorageService localStorageService)
{
    private const string StorageKey = Constants.McpServersKey;

    public async Task<List<McpSelection>?> GetConfig()
    {
        var data = await localStorageService.GetItemAsync<List<McpSelection>>(StorageKey);
        if (data != null)
            return data;
        return new List<McpSelection>();
    }

    public async Task SaveConfig(List<McpSelection> config)
    {
        await localStorageService.SetItemAsync(StorageKey, config);
    }

    public async Task ResetConfig()
    {
        await localStorageService.RemoveItemAsync(StorageKey);
    }
}
