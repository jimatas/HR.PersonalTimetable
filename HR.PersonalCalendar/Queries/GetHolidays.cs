using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Infrastructure;
using HR.WebUntisConnector;
using HR.WebUntisConnector.Extensions;
using HR.WebUntisConnector.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.Queries
{
    public class GetHolidays : IQuery<IEnumerable<Holiday>>
    {
        public string InstituteName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class GetHolidaysHandler : IQueryHandler<GetHolidays, IEnumerable<Holiday>>
    {
        private readonly IApiClientFactory apiClientFactory;

        public GetHolidaysHandler(ICachedApiClientFactory apiClientFactory)
            => this.apiClientFactory = Ensure.Argument.NotNull(() => apiClientFactory);

        public async Task<IEnumerable<Holiday>> HandleAsync(GetHolidays query, CancellationToken cancellationToken)
        {
            var apiClient = await apiClientFactory.CreateApiClientAndLogInAsync(query.InstituteName, cancellationToken).ConfigureAwait(false);
            try
            {
                var holidays = await apiClient.GetHolidaysAsync(cancellationToken).ConfigureAwait(false);
                var (startDate, endDate) = await EnsureStartAndEndDatesAsync(query.StartDate, query.EndDate, apiClient, cancellationToken).ConfigureAwait(false);
                
                holidays = holidays.Where(holiday => holiday.ToDateTimeRange().Overlaps(new(startDate, endDate)));
                return holidays;
            }
            finally
            {
                await apiClient.LogOutAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task<(DateTime StartDate, DateTime EndDate)> EnsureStartAndEndDatesAsync(DateTime? startDate, DateTime? endDate, IApiClient apiClient, CancellationToken cancellationToken)
        {
            if (startDate is null || endDate is null)
            {
                var currentSchoolYear = await apiClient.GetCurrentSchoolYearAsync(cancellationToken).ConfigureAwait(false);
                startDate ??= currentSchoolYear.GetStartDateTime();
                endDate ??= currentSchoolYear.GetEndDateTime();
            }
            return ((DateTime)startDate, (DateTime)endDate);
        }
    }
}
