using Developist.Core.Cqrs;
using Developist.Core.Cqrs.Commands;

using HR.PersonalTimetable.Api.Commands;
using HR.PersonalTimetable.Api.Extensions;
using HR.PersonalTimetable.Api.Models;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Api.Decorators
{
    public class VerifyUserNameHash : ICommandHandlerWrapper<AddPersonalTimetable>, IPrioritizable
    {
        public sbyte Priority => Priorities.Lower;

        public async Task HandleAsync(AddPersonalTimetable command, HandlerDelegate next, CancellationToken cancellationToken)
        {
            var userNameHash = command.UserName.ToSha256(command.Integration.CurrentSigningKey.Key);
            if (!userNameHash.Equals(command.UserNameHash, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedException("Hash did not match user name.");
            }

            await next().ConfigureAwait(false);
        }
    }
}
