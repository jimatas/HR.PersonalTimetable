using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Application.Extensions;
using HR.PersonalTimetable.Application.Services;
using HR.PersonalTimetable.Application.Validators;
using HR.WebUntisConnector.Extensions;

using Microsoft.AspNetCore.Mvc;
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

        public GetSchedulesForExportHandler(IQueryDispatcher queryDispatcher, IOptions<AppSettings> appSettings, IClock clock)
        {
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
            this.appSettings = Ensure.Argument.NotNull(() => appSettings).Value;
            this.clock = Ensure.Argument.NotNull(() => clock);
        }

        public async Task<FileContentResult> HandleAsync(GetSchedulesForExport query, CancellationToken cancellationToken)
        {
            var startDate = clock.Now.Date.GetFirstWeekday().AddDays(-(7 * appSettings.NumberOfWeeksBeforeCurrentToExport));
            var endDate = clock.Now.Date.GetLastWeekday().AddDays(7 * appSettings.NumberOfWeeksAfterCurrentToExport);

            var schedules = await queryDispatcher.DispatchAsync(new GetSchedules
            {
                UserName = query.UserName,
                StartDate = startDate,
                EndDate = endDate
            }, cancellationToken).WithoutCapturingContext();

            var calendarData = schedules.SelectMany(schedule => schedule.Lessons).ExportCalendar(appSettings.CalendarName, appSettings.ExportRefreshInterval, clock);
            return new FileContentResult(Encoding.UTF8.GetBytes(calendarData), contentType: "text/calendar")
            {
                FileDownloadName = appSettings.CalendarFileName,
                LastModified = clock.Now
            };
        }
    }
}
