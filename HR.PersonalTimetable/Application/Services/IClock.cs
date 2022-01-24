using System;

namespace HR.PersonalTimetable.Application.Services
{
    /// <summary>
    /// Abstracts the system clock to enable testing of time-bound code.
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// The current date/time in the computer's local time.
        /// </summary>
        public DateTimeOffset Now { get; }

        /// <summary>
        /// The current date/time in Coordinated Universal Time (UTC offset = 0).
        /// </summary>
        public DateTimeOffset UtcNow { get; }
    }
}
