namespace HR.PersonalTimetable.Application
{
    public class AppSettings
    {
        /// <summary>
        /// The maximum range of days for which timetables can be requested. 
        /// Too large of a value will excessively stress the server. Default value is 30 days.
        /// </summary>
        public int MaxTimetableRangeInDays { get; set; } = 30;

        /// <summary>
        /// The number of additional weeks prior to the current one to include in the calendar export.
        /// Default value is 1 week.
        /// Note that the date range created by <see cref="NumberOfWeeksBeforeCurrentToExport"/> and <see cref="NumberOfWeeksAfterCurrentToExport"/> must not exceed <see cref="MaxTimetableRangeInDays"/>.
        /// </summary>
        public int NumberOfWeeksBeforeCurrentToExport { get; set; } = 1;

        /// <summary>
        /// The number of additional weeks after the current one to include in the calendar export.
        /// Default value is 2 weeks.
        /// Note that the date range created by <see cref="NumberOfWeeksBeforeCurrentToExport"/> and <see cref="NumberOfWeeksAfterCurrentToExport"/> must not exceed <see cref="MaxTimetableRangeInDays"/>.
        /// </summary>
        public int NumberOfWeeksAfterCurrentToExport { get; set; } = 2;

        /// <summary>
        /// The suggested refresh interval in minutes that an external calendar should poll for calendar updates.
        /// Too small of a value will excessively stress the server. Default value is 30 minutes.
        /// </summary>
        public int ExportRefreshIntervalInMinutes { get; set; } = 30;

        /// <summary>
        /// The file download name to use for the calendar export.
        /// Default value is "HR_Rooster.ics"
        /// </summary>
        public string CalendarFileName { get; set; } = "HR_Rooster.ics";
    }
}
