using System;
using System.Net;

namespace HR.PersonalTimetable.Api.Models
{
    /// <summary>
    /// Thrown to indicate that the user attempted to access a restricted resource.
    /// </summary>
    public class UnauthorizedException : ApiException
    {
        public UnauthorizedException() 
            : base(HttpStatusCode.Unauthorized) { }
        public UnauthorizedException(string message) 
            : base(HttpStatusCode.Unauthorized, message) { }
        public UnauthorizedException(string message, Exception innerException) 
            : base(HttpStatusCode.Unauthorized, message, innerException) { }
    }
}
