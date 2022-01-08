namespace HR.PersonalTimetable.Api.Models
{
    public class AppSettings
    {
        /// <summary>
        /// The maximum range of days for which timetables can be requested. 
        /// Too large of a value might excessively stress the server. Default value is 30 days.
        /// </summary>
        public int MaxTimetableRangeInDays { get; set; } = 30;

        /// <summary>
        /// The number of additional weeks prior to the current one to include in a calendar export.
        /// Default value is 1 week.
        /// </summary>
        public int NumberOfWeeksBeforeCurrentToExport { get; set; } = 1;

        /// <summary>
        /// The number of additional weeks after the current one to include in a calendar export.
        /// Default value is 2 weeks.
        /// </summary>
        public int NumberOfWeeksAfterCurrentToExport { get; set; } = 2;

        /// <summary>
        /// The suggested refresh interval in minutes that an external calendar should poll for calendar updates.
        /// Default value is 30 minutes.
        /// </summary>
        public int ExportRefreshIntervalInMinutes { get; set; } = 30;
    }
}
