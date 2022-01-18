using Developist.Core.Cqrs;
using Developist.Core.Cqrs.Commands;
using Developist.Core.Persistence;
using Developist.Core.Persistence.Entities;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Commands;
using HR.PersonalTimetable.Api.Models;

using Microsoft.AspNetCore.Http;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Api.Decorators
{
    public class EnsureIntegration : ICommandHandlerWrapper<AddPersonalTimetable>, IPrioritizable
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IUnitOfWork unitOfWork;

        public EnsureIntegration(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
        {
            this.httpContextAccessor = Ensure.Argument.NotNull(() => httpContextAccessor);
            this.unitOfWork = Ensure.Argument.NotNull(() => unitOfWork);
        }

        public sbyte Priority => Priorities.Higher;

        public async Task HandleAsync(AddPersonalTimetable command, HandlerDelegate next, CancellationToken cancellationToken)
        {
            command.Integration = await GetIntegrationAsync(cancellationToken).ConfigureAwait(false);

            await next().ConfigureAwait(false);
        }

        private async Task<Integration> GetIntegrationAsync(CancellationToken cancellationToken)
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext.Request.Headers.TryGetValue(Integration.HeaderName, out var integrationName))
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
            throw new BadRequestException($"Required header \"{Integration.HeaderName}\" was not present.");
        }
    }
}
