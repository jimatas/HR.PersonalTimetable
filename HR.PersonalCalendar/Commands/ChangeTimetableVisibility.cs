using Developist.Core.Cqrs.Commands;
using Developist.Core.Persistence;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Infrastructure;
using HR.PersonalCalendar.Model;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.Commands
{
    public class ChangeTimetableVisibility : ICommand
    {
        public Guid PersonalTimetableId { get; set; }
        public bool IsVisible { get; set; }
        public string UserNameToVerify { get; set; }
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
            if (personalTimetable is not null && 
                personalTimetable.IsVisible != command.IsVisible &&
                personalTimetable.UserName.Equals(command.UserNameToVerify, StringComparison.InvariantCultureIgnoreCase))
            {
                personalTimetable.IsVisible = command.IsVisible;
                personalTimetable.DateLastModified = clock.Now;

                await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
