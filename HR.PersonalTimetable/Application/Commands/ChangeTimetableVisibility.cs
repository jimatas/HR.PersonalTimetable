using Developist.Core.Cqrs.Commands;
using Developist.Core.Persistence;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Application.Exceptions;
using HR.PersonalTimetable.Application.Models;
using HR.PersonalTimetable.Application.Services;

using Microsoft.AspNetCore.Mvc;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Application.Commands
{
    public class ChangeTimetableVisibility : ICommand
    {
        /// <summary>
        /// The unique identifier of the personal timetable.
        /// </summary>
        [Required]
        [FromRoute(Name = "id")]
        public Guid PersonalTimetableId { get; set; }

        [Required]
        [FromQuery(Name = "visible")]
        public bool IsVisible { get; set; }

        /// <summary>
        /// The integration through which the timetable is being edited.
        /// </summary>
        internal Integration Integration { get; set; }

        /// <summary>
        /// A salted hash of the username to verify.
        /// The salt is the (current) signing key of the integration through which the timetable is being edited.
        /// </summary>
        internal string UserNameToVerify { get; set; }
    }

    public class ChangeTimetableVisibilityHandler : ICommandHandler<ChangeTimetableVisibility>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IClock clock;

        public ChangeTimetableVisibilityHandler(IUnitOfWork unitOfWork, IClock clock)
        {
            this.unitOfWork = Ensure.Argument.NotNull(() => unitOfWork);
            this.clock = Ensure.Argument.NotNull(() => clock);
        }

        public async Task HandleAsync(ChangeTimetableVisibility command, CancellationToken cancellationToken)
        {
            var personalTimetable = await unitOfWork.Repository<Models.PersonalTimetable>().GetAsync(command.PersonalTimetableId, cancellationToken).ConfigureAwait(false);
            if (personalTimetable is null)
            {
                throw new NotFoundException($"No {nameof(PersonalTimetable)} with {nameof(Models.PersonalTimetable.Id)} {command.PersonalTimetableId} found.");
            }

            if (personalTimetable.VerifyAccess(command.UserNameToVerify, command.Integration.CurrentSigningKey) && personalTimetable.IsVisible != command.IsVisible)
            {
                personalTimetable.IsVisible = command.IsVisible;
                personalTimetable.DateLastModified = clock.Now;

                await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
