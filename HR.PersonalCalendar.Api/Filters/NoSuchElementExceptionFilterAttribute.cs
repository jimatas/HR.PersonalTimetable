using HR.PersonalCalendar.Api.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using System.Net;

namespace HR.PersonalCalendar.Api.Filters
{
    public class NoSuchElementExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is NoSuchElementException exception)
            {
                context.Result = new NotFoundObjectResult(new ProblemDetails
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Not Found",
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Detail = exception.Message
                });
                context.ExceptionHandled = true;
            }
        }
    }
}
