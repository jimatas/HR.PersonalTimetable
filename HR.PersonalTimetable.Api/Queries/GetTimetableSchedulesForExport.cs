using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Extensions;
using HR.PersonalTimetable.Api.Infrastructure;
using HR.PersonalTimetable.Api.Models;
using HR.PersonalTimetable.Api.Validators;
using HR.WebUntisConnector.Extensions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Api.Queries
{
    public class GetTimetableSchedulesForExport : IQuery<FileContentResult>
    {
        [Required, UserName]
        [FromRoute(Name = "user")]
        public string UserName { get; set; }
    }

    public class GetTimetableSchedulesForExportHandler : IQueryHandler<GetTimetableSchedulesForExport, FileContentResult>
    {
        private readonly IQueryDispatcher queryDispatcher;
        private readonly AppSettings appSettings;
        private readonly IClock clock;

        public GetTimetableSchedulesForExportHandler(IQueryDispatcher queryDispatcher, IOptions<AppSettings> appSettings, IClock clock)
        {
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
            this.appSettings = Ensure.Argument.NotNull(() => appSettings).Value;
            this.clock = Ensure.Argument.NotNull(() => clock);
        }

        public async Task<FileContentResult> HandleAsync(GetTimetableSchedulesForExport query, CancellationToken cancellationToken)
        {
            var startDate = clock.Now.Date.GetFirstWeekday().AddDays(-(7 * appSettings.NumberOfWeeksBeforeCurrentToExport));
            var endDate = clock.Now.Date.GetLastWeekday().AddDays(7 * appSettings.NumberOfWeeksAfterCurrentToExport);

            var schedules = await queryDispatcher.DispatchAsync(new GetTimetableSchedules
            {
                UserName = query.UserName,
                StartDate = startDate,
                EndDate = endDate
            }, cancellationToken).ConfigureAwait(false);

            var calendarData = schedules.SelectMany(schedule => schedule.TimetableGroups).ExportCalendar(appSettings.ExportRefreshIntervalInMinutes);
            return new FileContentResult(Encoding.UTF8.GetBytes(calendarData), contentType: "text/calendar")
            {
                FileDownloadName = appSettings.CalendarFileName,
                LastModified = clock.Now
            };
        }
    }
}
