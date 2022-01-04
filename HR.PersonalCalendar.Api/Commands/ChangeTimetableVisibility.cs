﻿using Developist.Core.Cqrs.Commands;
using Developist.Core.Persistence;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Api.Infrastructure;
using HR.PersonalCalendar.Api.Models;

using Microsoft.AspNetCore.Mvc;

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.Api.Commands
{
    [DisplayName("TimetableVisibility")]
    public class ChangeTimetableVisibility : ICommand
    {
        [Required]
        [FromRoute(Name = "id")]
        public Guid PersonalTimetableId { get; set; }

        [Required]
        [FromQuery(Name = "visible")]
        public bool IsVisible { get; set; }

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
            var personalTimetable = await unitOfWork.Repository<PersonalTimetable>().GetAsync(command.PersonalTimetableId, cancellationToken).ConfigureAwait(false);
            if (personalTimetable is not null && personalTimetable.VerifyAccess(command.UserNameToVerify) && personalTimetable.IsVisible != command.IsVisible)
            {
                personalTimetable.IsVisible = command.IsVisible;
                personalTimetable.DateLastModified = clock.Now;

                await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}