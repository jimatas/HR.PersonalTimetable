using System;

namespace HR.PersonalCalendar.Api.Models
{
    /// <summary>
    /// The exception thrown to indicate that a user attempted to access some restricted resource.
    /// </summary>
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException() { }
        public UnauthorizedException(string message) : base(message) { }
        public UnauthorizedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
