using HR.Common.Cqrs.Commands;
using HR.Common.Persistence;
using HR.Common.Utilities;
using HR.PersonalTimetable.Application.Exceptions;

using Microsoft.AspNetCore.Mvc;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Application.Commands
{
    public class RemovePersonalTimetable : AuthorizableCommandBase
    {
        /// <summary>
        /// The unique identifier of the personal timetable.
        /// </summary>
        [Required]
        [FromRoute(Name = "id")]
        public Guid PersonalTimetableId { get; set; }
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
            var personalTimetable = await unitOfWork.Repository<Models.PersonalTimetable>().GetAsync(command.PersonalTimetableId, cancellationToken);
            if (personalTimetable is null)
            {
                throw new NotFoundException($"No {nameof(PersonalTimetable)} with {nameof(Models.PersonalTimetable.Id)} {command.PersonalTimetableId} found.");
            }

            if (command.Authorization.VerifyAccess(personalTimetable))
            {
                unitOfWork.Repository<Models.PersonalTimetable>().Remove(personalTimetable);

                await unitOfWork.CompleteAsync(cancellationToken);
            }
        }
    }
}
