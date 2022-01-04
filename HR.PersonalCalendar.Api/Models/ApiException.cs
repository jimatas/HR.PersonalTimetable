using System;
using System.Net;

namespace HR.PersonalCalendar.Api.Models
{
    public class ApiException : Exception
    {
        public ApiException(HttpStatusCode statusCode)
            : base() => StatusCode = statusCode;

        public ApiException(HttpStatusCode statusCode, string message)
            : base(message) => StatusCode = statusCode;

        public ApiException(HttpStatusCode statusCode, string message, Exception innerException)
            : base(message, innerException) => StatusCode = statusCode;

        /// <summary>
        /// The status code to return to the client for this exception.
        /// </summary>
        public HttpStatusCode StatusCode { get; }
    }
}
