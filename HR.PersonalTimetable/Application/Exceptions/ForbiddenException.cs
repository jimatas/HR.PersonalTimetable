using System;
using System.Net;

namespace HR.PersonalTimetable.Application.Exceptions
{
    /// <summary>
    /// Thrown to indicate that the user does not have sufficient rights to access a particular restricted resource.
    /// This type of error indicates an authorization problem.
    /// </summary>
    public class ForbiddenException : ApiException
    {
        public ForbiddenException()
            : base(HttpStatusCode.Forbidden) { }
        public ForbiddenException(string message)
            : base(HttpStatusCode.Forbidden, message) { }
        public ForbiddenException(string message, Exception innerException)
            : base(HttpStatusCode.Forbidden, message, innerException) { }
    }
}
