using Developist.Core.Cqrs;
using Developist.Core.Cqrs.Commands;
using Developist.Core.Persistence;
using Developist.Core.Persistence.Entities;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Models;

using Microsoft.AspNetCore.Http;

using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Api.Decorators
{
    public class EnsureAuthorization<TCommand> : IPrioritizable, ICommandHandlerWrapper<TCommand>
        where TCommand : ICommand
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IUnitOfWork unitOfWork;

        public EnsureAuthorization(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
        {
            this.httpContextAccessor = Ensure.Argument.NotNull(() => httpContextAccessor);
            this.unitOfWork = Ensure.Argument.NotNull(() => unitOfWork);
        }

        public sbyte Priority => Priorities.Higher;

        public async Task HandleAsync(TCommand command, HandlerDelegate next, CancellationToken cancellationToken)
        {
            const BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var property = command.GetType().GetProperty("UserNameToVerify", bindingAttr);
            if (property is not null && string.IsNullOrEmpty(property.GetValue(command) as string))
            {
                property.SetValue(command, GetUserNameToVerify());
            }

            property = command.GetType().GetProperty("Integration", bindingAttr);
            if (property is not null && property.GetValue(command) is null)
            {
                property.SetValue(command, await GetIntegrationAsync(cancellationToken).ConfigureAwait(false));
            }

            await next().ConfigureAwait(false);
        }

        private string GetUserNameToVerify()
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext.Request.Headers.TryGetValue("X-HR-Authorization", out var userNameToVerify))
            {
                return userNameToVerify.ToString();
            }
            throw new BadRequestException("Required header \"X-HR-Authorization\" was not present.");
        }

        private async Task<Integration> GetIntegrationAsync(CancellationToken cancellationToken)
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext.Request.Headers.TryGetValue("X-HR-Integration", out var integrationName))
            {
                var integration = (await unitOfWork.Repository<Integration>().FindAsync(
                    integration => integration.Name == integrationName.ToString(),
                    includePaths => includePaths.Include(integration => integration.SigningKeys),
                    cancellationToken).ConfigureAwait(false)).SingleOrDefault() ?? throw new NotFoundException($"No integration with name \"{integrationName}\" found.");

                if (integration.SigningKeys.Any())
                {
                    return integration;
                }

                throw new NotFoundException($"No signing key found for integration with name \"{integration.Name}\".");
            }
            throw new BadRequestException("Required header \"X-HR-Integration\" was not present.");
        }
    }
}
