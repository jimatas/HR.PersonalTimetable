using Developist.Core.Cqrs.Commands;
using Developist.Core.Persistence;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Infrastructure;
using HR.PersonalCalendar.Model;
using HR.WebUntisConnector.Model;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.Commands
{
    public class ChangeTimetableVisibility : ICommand
    {
        public string UserName { get; set; }
        public string InstituteName { get; set; }
        public ElementType ElementType { get; set; }
        public string ElementName { get; set; }
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
            var personalTimetable = (await unitOfWork.Repository<PersonalTimetable>().FindAsync(new FilterByCommand(command), cancellationToken).ConfigureAwait(false)).FirstOrDefault();
            if (personalTimetable is not null && personalTimetable.IsVisible != command.IsVisible)
            {
                personalTimetable.IsVisible = command.IsVisible;
                personalTimetable.DateLastModified = clock.Now;

                await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private class FilterByCommand : IQueryableFilter<PersonalTimetable>
        {
            private readonly ChangeTimetableVisibility command;
            public FilterByCommand(ChangeTimetableVisibility command) => this.command = command;
            public IQueryable<PersonalTimetable> Filter(IQueryable<PersonalTimetable> sequence)
            {
                sequence = sequence.Where(table => table.UserName == command.UserName);
                sequence = sequence.Where(table => table.InstituteName == command.InstituteName);
                sequence = sequence.Where(table => table.ElementType == command.ElementType);
                sequence = sequence.Where(table => table.ElementName == command.ElementName);
                return sequence;
            }
        }
    }
}
