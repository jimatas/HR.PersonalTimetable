using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using System.ComponentModel.DataAnnotations;

namespace HR.PersonalCalendar.Api.Filters
{
    public class ValidationExceptionFilterAttribute: ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is ValidationException exception)
            {
                AddValidationResultToModelState(exception, context.ModelState);
                if (context.ModelState.ErrorCount != 0)
                {
                    context.Result = new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState)
                    {
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                    });
                }
                else
                {
                    context.Result = new BadRequestResult();
                }
                context.ExceptionHandled = true;
            }
        }

        private static void AddValidationResultToModelState(ValidationException exception, ModelStateDictionary modelState)
        {
            foreach (var memberName in exception.ValidationResult.MemberNames)
            {
                if (!modelState.ContainsKey(memberName))
                {
                    modelState.TryAddModelError(memberName, exception.Message);
                }
            }
        }
    }
}
