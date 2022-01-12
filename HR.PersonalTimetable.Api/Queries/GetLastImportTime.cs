using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Infrastructure;
using HR.WebUntisConnector;
using HR.WebUntisConnector.Extensions;

using Microsoft.AspNetCore.Mvc;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Api.Queries
{
    public class GetLastImportTime : IQuery<DateTime>
    {
        [Required]
        [FromRoute(Name = "institute")]
        public string InstituteName { get; set; }
    }

    public class GetLastImportTimeHandler : IQueryHandler<GetLastImportTime, DateTime>
    {
        private readonly IApiClientFactory apiClientFactory;

        public GetLastImportTimeHandler(ICachedApiClientFactory apiClientFactory)
        {
            this.apiClientFactory = Ensure.Argument.NotNull(() => apiClientFactory);
        }

        public async Task<DateTime> HandleAsync(GetLastImportTime query, CancellationToken cancellationToken)
        {
            var apiClient = await apiClientFactory.CreateApiClientAndLogInAsync(query.InstituteName, cancellationToken).ConfigureAwait(false);
            try
            {
                return await apiClient.GetLatestImportTimeAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await apiClient.LogOutAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
