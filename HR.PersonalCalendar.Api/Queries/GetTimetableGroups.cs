using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Api.Infrastructure;
using HR.WebUntisConnector;
using HR.WebUntisConnector.Extensions;
using HR.WebUntisConnector.Model;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.Api.Queries
{
    public class GetTimetableGroups : IQuery<IEnumerable<TimetableGroup>>
    {
        [Required]
        [FromQuery(Name = "institute")]
        public string InstituteName { get; set; }

        [Required]
        [FromQuery(Name = "element")]
        public ElementType ElementType { get; set; }

        [FromQuery(Name = "id")]
        public int? ElementId { get; set; }

        [FromQuery(Name = "name")]
        public string ElementName { get; set; }

        [FromQuery(Name = "start")]
        public DateTime? StartDate { get; set; }

        [FromQuery(Name = "end")]
        public DateTime? EndDate { get; set; }
    }

    public class GetTimetableGroupsHandler : IQueryHandler<GetTimetableGroups, IEnumerable<TimetableGroup>>
    {
        private readonly IApiClientFactory apiClientFactory;

        public GetTimetableGroupsHandler(ICachedApiClientFactory apiClientFactory)
        {
            this.apiClientFactory = Ensure.Argument.NotNull(() => apiClientFactory);
        }

        public async Task<IEnumerable<TimetableGroup>> HandleAsync(GetTimetableGroups query, CancellationToken cancellationToken)
        {
            var apiClient = await apiClientFactory.CreateApiClientAndLogInAsync(query.InstituteName, cancellationToken).ConfigureAwait(false);
            try
            {
                var (timetables, _) = await apiClient.GetTimetablesAsync(query.ElementType, (int)query.ElementId, (DateTime)query.StartDate, (DateTime)query.EndDate, cancellationToken).ConfigureAwait(false);
                return timetables.ToTimetableGroups(await apiClient.GetTimegridsAsync(cancellationToken).ConfigureAwait(false));
            }
            finally
            {
                await apiClient.LogOutAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
