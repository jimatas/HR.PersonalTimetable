using HR.PersonalCalendar.Model;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HR.PersonalCalendar.WebApi.Filters
{
    public class UnauthorizedExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is UnauthorizedException)
            {
                context.Result = new UnauthorizedResult();
                context.ExceptionHandled = true;
            }
        }
    }
}
