using Developist.Core.Cqrs.Commands;
using Developist.Core.Persistence;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Model;
using HR.WebUntisConnector.Model;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.Commands
{
    public class RemovePersonalTimetable : ICommand
    {
        public string UserName { get; set; }
        public string InstituteName { get; set; }
        public ElementType ElementType { get; set; }
        public string ElementName { get; set; }
    }

    public class RemovePersonalTimetableHandler : ICommandHandler<RemovePersonalTimetable>
    {
        private readonly IUnitOfWork unitOfWork;

        public RemovePersonalTimetableHandler(IUnitOfWork unitOfWork) 
            => this.unitOfWork = Ensure.Argument.NotNull(() => unitOfWork);

        public async Task HandleAsync(RemovePersonalTimetable command, CancellationToken cancellationToken)
        {
            var personalTimetable = (await unitOfWork.Repository<PersonalTimetable>().FindAsync(new FilterByCommand(command), cancellationToken).ConfigureAwait(false)).FirstOrDefault();
            if (personalTimetable is not null)
            {
                unitOfWork.Repository<PersonalTimetable>().Remove(personalTimetable);

                await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private class FilterByCommand : IQueryableFilter<PersonalTimetable>
        {
            private readonly RemovePersonalTimetable command;
            public FilterByCommand(RemovePersonalTimetable command) => this.command = command;
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
