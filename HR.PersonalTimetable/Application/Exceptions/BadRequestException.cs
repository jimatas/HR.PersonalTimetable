using System;
using System.Net;

namespace HR.PersonalTimetable.Application.Exceptions
{
    /// <summary>
    /// Thrown to indicate a generic request error. Typically caused by a failed validation constraint.
    /// </summary>
    public class BadRequestException : ApiException
    {
        public BadRequestException()
            : base(HttpStatusCode.BadRequest) { }
        public BadRequestException(string message)
            : base(HttpStatusCode.BadRequest, message) { }
        public BadRequestException(string message, Exception innerException)
            : base(HttpStatusCode.BadRequest, message, innerException) { }
    }
}
