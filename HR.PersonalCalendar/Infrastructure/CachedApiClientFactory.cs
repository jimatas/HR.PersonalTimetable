using Developist.Core.Utilities;

using HR.WebUntisConnector;

using System.Collections.Generic;
using System.Threading;

namespace HR.PersonalCalendar.Infrastructure
{
    /// <summary>
    /// Wraps an actual <see cref="IApiClientFactory"/> implementation and caches the <see cref="IApiClient"/> objects that are created by it.
    /// </summary>
    public class CachedApiClientFactory : ICachedApiClientFactory
    {
        private readonly IApiClientFactory apiClientFactory;
        private readonly IDictionary<string, CachedApiClient> cachedApiClients = new Dictionary<string, CachedApiClient>();
        private readonly SemaphoreSlim mutex = new(1, 1);

        public CachedApiClientFactory(IApiClientFactory apiClientFactory)
            => this.apiClientFactory = Ensure.Argument.NotNull(() => apiClientFactory);

        /// <inheritdoc/>
        public IApiClient CreateApiClient(string schoolOrInstituteName, out string userName, out string password)
        {
            IApiClient apiClient;

            mutex.Wait();
            try
            {
                if (cachedApiClients.TryGetValue(schoolOrInstituteName, out var cachedApiClient))
                {
                    (apiClient, userName, password) = cachedApiClient;
                }
                else
                {
                    apiClient = apiClientFactory.CreateApiClient(schoolOrInstituteName, out userName, out password);
                    cachedApiClients.Add(schoolOrInstituteName, new(apiClient, userName, password));
                }
            }
            finally
            {
                mutex.Release();
            }

            return apiClient;
        }

        private record CachedApiClient(IApiClient ApiClient, string UserName, string Password);
    }
}
