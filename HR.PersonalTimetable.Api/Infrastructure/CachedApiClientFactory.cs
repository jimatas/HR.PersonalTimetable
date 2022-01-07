using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Extensions;
using HR.PersonalTimetable.Api.Models;
using HR.WebUntisConnector;
using HR.WebUntisConnector.Configuration;

using System.Collections.Generic;
using System.Threading;

namespace HR.PersonalTimetable.Api.Infrastructure
{
    /// <summary>
    /// Wraps an actual <see cref="IApiClientFactory"/> implementation and caches the <see cref="IApiClient"/> objects that are created by it.
    /// </summary>
    public class CachedApiClientFactory : ICachedApiClientFactory
    {
        private readonly IApiClientFactory apiClientFactory;
        private readonly WebUntisConfigurationSection configuration;
        private readonly IDictionary<string, CachedApiClient> cachedApiClients = new Dictionary<string, CachedApiClient>();
        private readonly SemaphoreSlim mutex = new(1, 1);

        public CachedApiClientFactory(IApiClientFactory apiClientFactory, WebUntisConfigurationSection configuration)
        {
            this.apiClientFactory = Ensure.Argument.NotNull(() => apiClientFactory);
            this.configuration = Ensure.Argument.NotNull(() => configuration);
        }

        /// <inheritdoc/>
        public IApiClient CreateApiClient(string schoolOrInstituteName, out string userName, out string password)
        {
            IApiClient apiClient;
            var school = configuration.FindSchool(schoolOrInstituteName)
                ?? throw new NotFoundException($"No school or institute with the name {schoolOrInstituteName} found.");

            using (mutex.WaitAndRelease())
            {
                if (cachedApiClients.TryGetValue(school.Name, out var cachedApiClient))
                {
                    (apiClient, userName, password) = cachedApiClient;
                }
                else
                {
                    apiClient = apiClientFactory.CreateApiClient(school.Name, out userName, out password);
                    cachedApiClients.Add(school.Name, new(apiClient, userName, password));
                }
            }
            return apiClient;
        }

        private record CachedApiClient(IApiClient ApiClient, string UserName, string Password);
    }
}
