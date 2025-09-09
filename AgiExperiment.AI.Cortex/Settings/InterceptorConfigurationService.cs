using AgiExperiment.AI.Cortex.Common;
using AgiExperiment.AI.Cortex.Pipeline.Interceptors;

namespace AgiExperiment.AI.Cortex.Settings;

public class InterceptorConfigurationService(ILocalStorageService localStorageService)
{
    private const string StorageKey = Constants.InterceptorsKey;
    private readonly ILocalStorageService? _localStorageService = localStorageService;

    public async Task<IEnumerable<string>> GetConfig()
    {
        var model = await _localStorageService!.GetItemAsync<IEnumerable<string>?>(StorageKey);
        return model ?? Array.Empty<string>();
    }

    public async Task SaveConfig(IEnumerable<string> config)
    {
        await _localStorageService!.SetItemAsync(StorageKey, config);
    }

    public async Task ResetConfig()
    {
        await _localStorageService!.RemoveItemAsync(StorageKey);
    }
}