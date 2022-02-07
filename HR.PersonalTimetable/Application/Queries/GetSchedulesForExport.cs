using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Application.Extensions;
using HR.PersonalTimetable.Application.Services;
using HR.PersonalTimetable.Application.Validators;
using HR.PersonalTimetable.Resources;
using HR.WebUntisConnector.Extensions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Application.Queries
{
    public class GetSchedulesForExport : IQuery<FileContentResult>
    {
        /// <summary>
        /// The username of the user. 
        /// Depending on their role, either an employee code or a student number.
        /// </summary>
        [Required, UserName]
        [FromRoute(Name = "user")]
        public string UserName { get; set; }
    }

    public class GetSchedulesForExportHandler : IQueryHandler<GetSchedulesForExport, FileContentResult>
    {
        private readonly IQueryDispatcher queryDispatcher;
        private readonly AppSettings appSettings;
        private readonly IClock clock;
        private readonly IStringLocalizer localizer;

        public GetSchedulesForExportHandler(IQueryDispatcher queryDispatcher, IOptions<AppSettings> appSettings, IClock clock, IStringLocalizer<SharedResource> localizer)
        {
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
            this.appSettings = Ensure.Argument.NotNull(() => appSettings).Value;
            this.clock = Ensure.Argument.NotNull(() => clock);
            this.localizer = Ensure.Argument.NotNull(() => localizer);
        }

        public async Task<FileContentResult> HandleAsync(GetSchedulesForExport query, CancellationToken cancellationToken)
        {
            var startDate = clock.Now.Date.GetFirstWeekday().AddDays(-(7 * appSettings.CalendarWeeksInPast));
            var endDate = clock.Now.Date.GetLastWeekday().AddDays(7 * appSettings.CalendarWeeksInFuture);

            var schedules = await queryDispatcher.DispatchAsync(new GetSchedules
            {
                UserName = query.UserName,
                StartDate = startDate,
                EndDate = endDate
            }, cancellationToken);

            var calendarData = schedules.SelectMany(schedule => schedule.Lessons).ExportCalendar(appSettings.CalendarRefreshInterval, clock, localizer);
            return new FileContentResult(Encoding.UTF8.GetBytes(calendarData), contentType: "text/calendar")
            {
                FileDownloadName = localizer["CalendarFileName"],
                LastModified = clock.Now
            };
        }
    }
}
