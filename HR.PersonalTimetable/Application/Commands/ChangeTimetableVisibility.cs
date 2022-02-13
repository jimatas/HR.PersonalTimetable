using Developist.Core.Cqrs.Commands;
using Developist.Core.Persistence;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Application.Exceptions;
using HR.PersonalTimetable.Application.Services;

using Microsoft.AspNetCore.Mvc;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Application.Commands
{
    public class ChangeTimetableVisibility : AuthorizableCommandBase
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
            var personalTimetable = await unitOfWork.Repository<Models.PersonalTimetable>().GetAsync(command.PersonalTimetableId, cancellationToken);
            if (personalTimetable is null)
            {
                throw new NotFoundException($"No {nameof(PersonalTimetable)} with {nameof(Models.PersonalTimetable.Id)} {command.PersonalTimetableId} found.");
            }

            if (command.Authorization.VerifyAccess(personalTimetable) && personalTimetable.IsVisible != command.IsVisible)
            {
                personalTimetable.IsVisible = command.IsVisible;
                personalTimetable.DateLastModified = clock.Now;

                await unitOfWork.CompleteAsync(cancellationToken);
            }
        }
    }
}
