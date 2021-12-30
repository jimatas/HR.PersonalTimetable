using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Infrastructure;
using HR.WebUntisConnector;
using HR.WebUntisConnector.Extensions;
using HR.WebUntisConnector.Model;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.Queries
{
    public class GetTimetableGroups : IQuery<IEnumerable<TimetableGroup>>
    {
        public string InstituteName { get; set; }
        public Element Element { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class GetTimetableGroupsHandler : IQueryHandler<GetTimetableGroups, IEnumerable<TimetableGroup>>
    {
        private readonly IApiClientFactory apiClientFactory;

        public GetTimetableGroupsHandler(ICachedApiClientFactory apiClientFactory)
            => this.apiClientFactory = Ensure.Argument.NotNull(() => apiClientFactory);

        public async Task<IEnumerable<TimetableGroup>> HandleAsync(GetTimetableGroups query, CancellationToken cancellationToken)
        {
            var apiClient = await apiClientFactory.CreateApiClientAndLogInAsync(query.InstituteName, cancellationToken).ConfigureAwait(false);
            try
            {
                var (timetables, _) = await apiClient.GetTimetablesAsync(query.Element.Type, query.Element.Id, query.StartDate, query.EndDate, cancellationToken).ConfigureAwait(false);
                return timetables.ToTimetableGroups(await apiClient.GetTimegridsAsync(cancellationToken).ConfigureAwait(false));
            }
            finally
            {
                await apiClient.LogOutAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
