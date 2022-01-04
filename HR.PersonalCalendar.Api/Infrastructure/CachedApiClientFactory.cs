using Developist.Core.Utilities;

using HR.PersonalCalendar.Api.Extensions;
using HR.PersonalCalendar.Api.Models;
using HR.WebUntisConnector;
using HR.WebUntisConnector.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HR.PersonalCalendar.Api.Infrastructure
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

            using (mutex.WaitAndRelease())
            {
                if (cachedApiClients.TryGetValue(schoolOrInstituteName, out var cachedApiClient))
                {
                    (apiClient, userName, password) = cachedApiClient;
                }
                else
                {
                    EnsureSchoolOrInstituteExists(schoolOrInstituteName);
                    apiClient = apiClientFactory.CreateApiClient(schoolOrInstituteName, out userName, out password);
                    cachedApiClients.Add(schoolOrInstituteName, new(apiClient, userName, password));
                }
            }

            return apiClient;
        }

        /// <summary>
        /// Throws <see cref="NotFoundException"/> if <paramref name="schoolOrInstituteName"/> does not refer to a valid school or institute configuration element.
        /// </summary>
        /// <param name="schoolOrInstituteName"></param>
        private void EnsureSchoolOrInstituteExists(string schoolOrInstituteName)
        {
            if (!configuration.Schools.Any(school => school.Name.Equals(schoolOrInstituteName, StringComparison.InvariantCultureIgnoreCase) || school.Institutes.Any(institute => institute.Name.Equals(schoolOrInstituteName, StringComparison.InvariantCultureIgnoreCase))))
            {
                throw new NotFoundException($"No school or institute with the name {schoolOrInstituteName} found.");
            }
        }

        private record CachedApiClient(IApiClient ApiClient, string UserName, string Password);
    }
}
