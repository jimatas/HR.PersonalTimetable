using Developist.Core.Cqrs;
using Developist.Core.Cqrs.Commands;
using Developist.Core.Utilities;

using Microsoft.AspNetCore.Http;

using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Api.Commands
{
    public class UserNameToVerifyPopulator<TCommand> : ICommandHandlerWrapper<TCommand>
        where TCommand : ICommand
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public UserNameToVerifyPopulator(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = Ensure.Argument.NotNull(() => httpContextAccessor);
        }

        public async Task HandleAsync(TCommand command, HandlerDelegate next, CancellationToken cancellationToken)
        {
            var property = typeof(TCommand).GetProperty("UserNameToVerify", bindingAttr: BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property is not null && string.IsNullOrEmpty(property.GetValue(command) as string))
            {
                var httpContext = httpContextAccessor.HttpContext;
                if (httpContext.User.Identity.IsAuthenticated)
                {
                    property.SetValue(command, httpContext.User.Identity.Name);
                }
            }

            await next().ConfigureAwait(false);
        }
    }
}
