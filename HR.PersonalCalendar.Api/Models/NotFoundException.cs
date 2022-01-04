using System;
using System.Net;

namespace HR.PersonalCalendar.Api.Models
{
    /// <summary>
    /// Thrown to indicate that a requested resource does not exist or could not be found using the specified information.
    /// </summary>
    public class NotFoundException : ApiException
    {
        public NotFoundException() 
            : base(HttpStatusCode.NotFound) { }
        public NotFoundException(string message) 
            : base(HttpStatusCode.NotFound, message) { }
        public NotFoundException(string message, Exception innerException) 
            : base(HttpStatusCode.NotFound, message, innerException) { }
    }
}
