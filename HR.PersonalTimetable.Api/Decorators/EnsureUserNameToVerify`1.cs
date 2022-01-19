using Developist.Core.Cqrs;
using Developist.Core.Cqrs.Commands;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Commands;
using HR.PersonalTimetable.Api.Models;

using Microsoft.AspNetCore.Http;

using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Api.Decorators
{
    public class EnsureUserNameToVerify : IPrioritizable,
        ICommandHandlerWrapper<ChangeTimetableVisibility>,
        ICommandHandlerWrapper<RemovePersonalTimetable>
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public EnsureUserNameToVerify(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = Ensure.Argument.NotNull(() => httpContextAccessor);
        }

        public sbyte Priority => Priorities.High;

        public Task HandleAsync(ChangeTimetableVisibility command, HandlerDelegate next, CancellationToken cancellationToken)
        {
            command.UserNameToVerify = GetAuthorizationHeaderValue();

            return next();
        }

        public Task HandleAsync(RemovePersonalTimetable command, HandlerDelegate next, CancellationToken cancellationToken)
        {
            command.UserNameToVerify = GetAuthorizationHeaderValue();

            return next();
        }

        private string GetAuthorizationHeaderValue()
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext.Request.Headers.TryGetValue("X-HR-Authorization", out var authorization))
            {
                return authorization.ToString();
            }
            throw new BadRequestException("Required header \"X-HR-Authorization\" was not present.");
        }
    }
}
