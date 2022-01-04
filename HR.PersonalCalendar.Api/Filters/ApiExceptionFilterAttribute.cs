using HR.PersonalCalendar.Api.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace HR.PersonalCalendar.Api.Filters
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is ApiException exception)
            {
                var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                var problemDetails = problemDetailsFactory.CreateProblemDetails(context.HttpContext, statusCode: (int)exception.StatusCode, detail: exception.Message);
                
                context.Result = new ObjectResult(problemDetails);
                context.ExceptionHandled = true;
            }

            base.OnException(context);
        }
    }
}
