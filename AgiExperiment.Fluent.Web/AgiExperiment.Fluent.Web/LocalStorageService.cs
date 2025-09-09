using AgiExperiment.AI.Cortex.Pipeline.Interceptors;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace AgiExperiment.Fluent.Web
{
    public class LocalStorageService : ILocalStorageService
    {
        private readonly ProtectedLocalStorage protectedLocalStorage;

        public LocalStorageService(ProtectedLocalStorage protectedLocalStorage)
        {
            this.protectedLocalStorage = protectedLocalStorage;
        }

        public async Task<bool> ContainKeyAsync(string storageKey)
        {
            var value = await protectedLocalStorage.GetAsync<object>(storageKey);
            return value.Success;
        }

        public async Task<T> GetItemAsync<T>(string storageKey, CancellationToken cancellationToken)
        {
            var res = await protectedLocalStorage.GetAsync<T>(storageKey);
            return res.Success ? res.Value : default;
        }

        public async Task<T> GetItemAsync<T>(string storageKey)
        {
            return await GetItemAsync<T>(storageKey, CancellationToken.None);
        }

        public async Task RemoveItemAsync(string storageKey)
        {
            await protectedLocalStorage.DeleteAsync(storageKey);
        }

        public async Task SetItemAsync<T>(string storageKey, T value)
        {
            await protectedLocalStorage.SetAsync(storageKey, value);
        }
    }
}
