using System;

namespace HR.PersonalTimetable.Application
{
    public class AppSettings
    {
        /// <summary>
        /// The maximum date range for which timetables can be requested.
        /// Too large of a value will excessively stress the server. Default value is 30 days.
        /// </summary>
        public int TimetableMaxDays { get; set; } = 30;

        /// <summary>
        /// The number of additional weeks prior to the current one to include in the calendar export.
        /// Default value is 1 week.
        /// Note that the date range created by <see cref="CalendarWeeksInPast"/> and <see cref="CalendarWeeksInFuture"/> must not exceed <see cref="TimetableMaxDays"/>.
        /// </summary>
        public int CalendarWeeksInPast { get; set; } = 1;

        /// <summary>
        /// The number of additional weeks after the current one to include in the calendar export.
        /// Default value is 2 weeks.
        /// Note that the date range created by <see cref="CalendarWeeksInPast"/> and <see cref="CalendarWeeksInFuture"/> must not exceed <see cref="TimetableMaxDays"/>.
        /// </summary>
        public int CalendarWeeksInFuture { get; set; } = 2;

        /// <summary>
        /// The suggested refresh interval that an external calendar should poll for calendar updates.
        /// Too small of a value will excessively stress the server. Default value is 30 minutes.
        /// </summary>
        public TimeSpan CalendarRefreshInterval { get; set; } = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Clock skew to account for between client and server.
        /// Default value is 1 minute.
        /// </summary>
        /// <remarks>
        /// See also the <c>"X-HR-Timestamp"</c> request header.
        /// </remarks>
        public TimeSpan ClockSkewTolerance { get; set; } = TimeSpan.FromMinutes(1);
    }
}
