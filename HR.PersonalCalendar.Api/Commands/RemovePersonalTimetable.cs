﻿using Developist.Core.Cqrs.Commands;
using Developist.Core.Persistence;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Api.Models;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.Api.Commands
{
    public class RemovePersonalTimetable : ICommand
    {
        [FromRoute(Name = "id")]
        public Guid PersonalTimetableId { get; set; }

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
            var personalTimetable = await unitOfWork.Repository<PersonalTimetable>().GetAsync(command.PersonalTimetableId, cancellationToken).ConfigureAwait(false);
            if (personalTimetable is not null && personalTimetable.VerifyAccess(command.UserNameToVerify))
            {
                unitOfWork.Repository<PersonalTimetable>().Remove(personalTimetable);

                await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}