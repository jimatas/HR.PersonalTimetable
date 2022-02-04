using Developist.Core.Utilities;

using HR.PersonalTimetable.Application.Exceptions;
using HR.PersonalTimetable.Application.Extensions;
using HR.PersonalTimetable.Application.Services;
using HR.PersonalTimetable.Infrastructure.Extensions;
using HR.WebUntisConnector;
using HR.WebUntisConnector.Configuration;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Infrastructure.Services
{
    /// <summary>
    /// Wraps an actual <see cref="IApiClientFactory"/> implementation and caches the <see cref="IApiClient"/> objects that are created by it.
    /// </summary>
    public class CachedApiClientFactory : AsyncDisposableBase, ICachedApiClientFactory
    {
        private readonly IApiClientFactory apiClientFactory;
        private readonly WebUntisConfigurationSection configuration;
        private readonly IDictionary<string, CachedApiClientEntry> cachedApiClients = new Dictionary<string, CachedApiClientEntry>();
        private readonly SemaphoreSlim mutex = new(1, 1);

        public CachedApiClientFactory(IApiClientFactory apiClientFactory, WebUntisConfigurationSection configuration)
        {
            this.apiClientFactory = Ensure.Argument.NotNull(() => apiClientFactory);
            this.configuration = Ensure.Argument.NotNull(() => configuration);
        }

        /// <inheritdoc/>
        public IApiClient CreateApiClient(string schoolOrInstituteName, out string userName, out string password)
        {
            var school = configuration.FindSchool(schoolOrInstituteName)
                ?? throw new NotFoundException($"No school or institute with the name \"{schoolOrInstituteName}\" found.");

            CachedApiClient apiClient;
            using (mutex.WaitAndRelease())
            {
                if (cachedApiClients.TryGetValue(school.Name, out var cachedApiClient))
                {
                    (apiClient, userName, password) = cachedApiClient;
                }
                else
                {
                    apiClient = new CachedApiClient(apiClientFactory.CreateApiClient(school.Name, out userName, out password), schoolOrInstituteName);
                    cachedApiClients.Add(school.Name, new(apiClient, userName, password));
                }
            }
            return apiClient;
        }

        /// <inheritdoc/>
        protected override async ValueTask ReleaseManagedResourcesAsync()
        {
            foreach (var apiClient in cachedApiClients.Values.Select(entry => entry.ApiClient))
            {
                if (apiClient.IsAuthenticated)
                {
                    await apiClient.LogOutAsync(force: true).ConfigureAwait(false);
                }
            }
            cachedApiClients.Clear();

            await base.ReleaseManagedResourcesAsync().ConfigureAwait(false);
        }

        private record CachedApiClientEntry(CachedApiClient ApiClient, string UserName, string Password);
    }
}
