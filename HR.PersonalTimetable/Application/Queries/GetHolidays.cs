using HR.Common.Cqrs.Queries;
using HR.Common.Utilities;
using HR.PersonalTimetable.Application.Services;
using HR.WebUntisConnector;
using HR.WebUntisConnector.Extensions;
using HR.WebUntisConnector.Model;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Application.Queries
{
    public class GetHolidays : IQuery<IEnumerable<Holiday>>
    {
        /// <summary>
        /// The RUAS institute name.
        /// Alternatively, a WebUntis school name may be supplied.
        /// </summary>
        [Required]
        [FromRoute(Name = "institute")]
        public string InstituteName { get; set; }

        /// <summary>
        /// Optional start of the date range in which the returned holidays must fall.
        /// Unbounded, if not specified.
        /// </summary>
        [FromQuery(Name = "start")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Optional end of the date range in which the returned holidays must fall.
        /// Unbounded, if not specified.
        /// </summary>
        [FromQuery(Name = "end")]
        public DateTime? EndDate { get; set; }
    }

    public class GetHolidaysHandler : IQueryHandler<GetHolidays, IEnumerable<Holiday>>
    {
        private readonly IApiClientFactory apiClientFactory;

        public GetHolidaysHandler(ICachedApiClientFactory apiClientFactory)
        {
            this.apiClientFactory = Ensure.Argument.NotNull(() => apiClientFactory);
        }

        public async Task<IEnumerable<Holiday>> HandleAsync(GetHolidays query, CancellationToken cancellationToken)
        {
            var apiClient = await apiClientFactory.CreateApiClientAndLogInAsync(query.InstituteName, cancellationToken);
            try
            {
                return (await apiClient.GetHolidaysAsync(cancellationToken))
                    .Where(holiday => holiday.ToDateTimeRange().Overlaps(new((DateTime)query.StartDate, (DateTime)query.EndDate)));
            }
            finally
            {
                await apiClient.LogOutAsync(cancellationToken);
            }
        }
    }
}
