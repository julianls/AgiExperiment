

namespace AgiExperiment.AI.Cortex.Pipeline.Interceptors
{
    public interface ILocalStorageService
    {
        Task<bool> ContainKeyAsync(string storageKey);
        Task<T> GetItemAsync<T>(string storageKey, CancellationToken cancellationToken);
        Task<T> GetItemAsync<T>(string storageKey);
        Task RemoveItemAsync(string storageKey);
        Task SetItemAsync<T>(string storageKey, T value);
    }
}