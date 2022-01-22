using Developist.Core.Cqrs;
using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Extensions;
using HR.PersonalTimetable.Api.Infrastructure;
using HR.PersonalTimetable.Api.Models;
using HR.PersonalTimetable.Api.Queries;
using HR.WebUntisConnector.Extensions;
using HR.WebUntisConnector.Model;

using Microsoft.Extensions.Options;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Api.Decorators
{
    public class EnsureStartAndEndDate : IQueryHandlerWrapper<GetSchedule, Schedule>
    {
        private readonly AppSettings appSettings;
        private readonly IClock clock;

        public EnsureStartAndEndDate(IOptions<AppSettings> appSettings, IClock clock)
        {
            this.appSettings = Ensure.Argument.NotNull(() => appSettings).Value;
            this.clock = Ensure.Argument.NotNull(() => clock);
        }

        public async Task<Schedule> HandleAsync(GetSchedule query, HandlerDelegate<Schedule> next, CancellationToken cancellationToken)
        {
            if (query.StartDate.IsNullOrDefault())
            {
                query.StartDate = clock.Now.Date.GetFirstWeekday();
            }

            if (query.EndDate.IsNullOrDefault())
            {
                query.EndDate = clock.Now.Date.GetLastWeekday().AddDays(1);
            }

            if (new DateTimeRange((DateTime)query.StartDate, (DateTime)query.EndDate).Duration.TotalDays > appSettings.MaxTimetableRangeInDays)
            {
                throw new BadRequestException($"Requested timetable range too large. Maximum of {appSettings.MaxTimetableRangeInDays} days allowed.");
            }

            return await next().ConfigureAwait(false);
        }
    }
}
