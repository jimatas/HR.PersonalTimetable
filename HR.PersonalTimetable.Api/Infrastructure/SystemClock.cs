using System;

namespace HR.PersonalTimetable.Api.Infrastructure
{
    /// <summary>
    /// Default <see cref="IClock"/> implementation that encapsulates the system clock.
    /// </summary>
    public class SystemClock : IClock
    {
        /// <inheritdoc/>
        public DateTimeOffset Now => DateTimeOffset.Now;

        /// <inheritdoc/>
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
