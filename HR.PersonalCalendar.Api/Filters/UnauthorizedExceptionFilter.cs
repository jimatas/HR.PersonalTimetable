using HR.PersonalCalendar.Api.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using System.Net;

namespace HR.PersonalCalendar.Api.Filters
{
    public class UnauthorizedExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is UnauthorizedException exception)
            {
                context.Result = new UnauthorizedObjectResult(new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Unauthorized,
                    Title = "Unauthorized",
                    Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    Detail = exception.Message
                });
                context.ExceptionHandled = true;
            }
        }
    }
}
