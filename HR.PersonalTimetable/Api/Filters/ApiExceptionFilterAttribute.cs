using Developist.Core.Utilities;

using HR.PersonalTimetable.Application;
using HR.PersonalTimetable.Application.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System.Net;

namespace HR.PersonalTimetable.Api.Filters
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            var apiException = context.Exception as ApiException;
            AppSettings appSettings = null;

            if (apiException is null)
            {
                appSettings = context.HttpContext.RequestServices.GetRequiredService<IOptions<AppSettings>>().Value;
            }

            if (apiException is not null || appSettings.DiscloseInternalServerErrorDetails)
            {
                var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                var problemDetails = problemDetailsFactory.CreateProblemDetails(context.HttpContext, 
                    statusCode: (int)(apiException?.StatusCode ?? HttpStatusCode.InternalServerError), 
                    detail: apiException?.Message ?? context.Exception.DetailMessage());

                context.Result = new ObjectResult(problemDetails);
                context.ExceptionHandled = true;
            }

            base.OnException(context);
        }
    }
}
