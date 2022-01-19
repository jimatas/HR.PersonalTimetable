using Developist.Core.Cqrs.Commands;
using Developist.Core.Persistence;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Models;

using Microsoft.AspNetCore.Mvc;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Api.Commands
{
    public class RemovePersonalTimetable : ICommand
    {
        /// <summary>
        /// The unique identifier of the personal timetable.
        /// </summary>
        [Required]
        [FromRoute(Name = "id")]
        public Guid PersonalTimetableId { get; set; }

        /// <summary>
        /// The integration through which the timetable is being deleted.
        /// </summary>
        internal Integration Integration { get; set; }

        /// <summary>
        /// A salted hash of the username to verify.
        /// The salt is the (current) signing key of the integration through which the timetable is being deleted.
        /// </summary>
        internal string UserNameToVerify { get; set; }
    }

    public class RemovePersonalTimetableHandler : ICommandHandler<RemovePersonalTimetable>
    {
        private readonly IUnitOfWork unitOfWork;

        public RemovePersonalTimetableHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = Ensure.Argument.NotNull(() => unitOfWork);
        }

        public async Task HandleAsync(RemovePersonalTimetable command, CancellationToken cancellationToken)
        {
            var personalTimetable = await unitOfWork.Repository<Models.PersonalTimetable>().GetAsync(command.PersonalTimetableId, cancellationToken).ConfigureAwait(false);
            if (personalTimetable is null)
            {
                throw new NotFoundException($"No {nameof(PersonalTimetable)} with {nameof(Models.PersonalTimetable.Id)} {command.PersonalTimetableId} found.");
            }

            if (personalTimetable.VerifyAccess(command.UserNameToVerify, command.Integration.CurrentSigningKey))
            {
                unitOfWork.Repository<Models.PersonalTimetable>().Remove(personalTimetable);

                await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
