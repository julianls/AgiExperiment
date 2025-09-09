using System.Text.Json;
using AgiExperiment.AI.Cortex.Common;
using AgiExperiment.AI.Cortex.Pipeline.Interceptors;
using AgiExperiment.AI.Cortex.Settings.PluginSelector;

namespace AgiExperiment.AI.Cortex.Settings;

public class PluginsConfigurationService(ILocalStorageService localStorageService, SettingsStateNotificationService settingsState )
{
    private const string StorageKey = Constants.PluginsKey;

    public async Task<List<PluginSelection>?> GetConfig()
    {
        var data = await localStorageService.GetItemAsync<List<PluginSelection>>(StorageKey);
        if (data != null)
            return data;

        return new List<PluginSelection>();
    }

    public async Task SaveConfig(List<PluginSelection> config)
    {
        await localStorageService.SetItemAsync(StorageKey, config);
    }

    public async Task ResetConfig()
    {
        await localStorageService.RemoveItemAsync(StorageKey);
    }
}