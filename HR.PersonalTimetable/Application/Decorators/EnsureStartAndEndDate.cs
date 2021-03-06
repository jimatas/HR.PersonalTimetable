using HR.Common.Cqrs;
using HR.Common.Cqrs.Queries;
using HR.Common.Utilities;
using HR.PersonalTimetable.Application.Exceptions;
using HR.PersonalTimetable.Application.Models;
using HR.PersonalTimetable.Application.Queries;
using HR.PersonalTimetable.Application.Services;
using HR.WebUntisConnector.Extensions;
using HR.WebUntisConnector.Model;

using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Application.Decorators
{
    public class EnsureStartAndEndDate : IQueryHandlerWrapper<GetSchedule, Schedule>, IQueryHandlerWrapper<GetHolidays, IEnumerable<Holiday>>
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

            if (new DateTimeRange((DateTime)query.StartDate, (DateTime)query.EndDate).Duration.TotalDays > appSettings.TimetableMaxDays)
            {
                throw new BadRequestException($"Requested date range too large. Maximum of {appSettings.TimetableMaxDays} days allowed.");
            }

            return await next();
        }

        public Task<IEnumerable<Holiday>> HandleAsync(GetHolidays query, HandlerDelegate<IEnumerable<Holiday>> next, CancellationToken cancellationToken)
        {
            query.StartDate ??= DateTime.MinValue;
            query.EndDate ??= DateTime.MaxValue;

            return next();
        }
    }
}
