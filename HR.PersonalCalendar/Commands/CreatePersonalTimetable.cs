using Developist.Core.Cqrs.Commands;
using Developist.Core.Persistence;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Infrastructure;
using HR.PersonalCalendar.Model;
using HR.WebUntisConnector.Model;

using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.Commands
{
    public class CreatePersonalTimetable : ICommand
    {
        public string UserName { get; set; }
        public ElementType ElementType { get; set; }
        public string ElementName { get; set; }
        public string InstituteName { get; set; }
        public int? SchoolYearId { get; set; }
    }

    public class CreatePersonalTimetableHandler : ICommandHandler<CreatePersonalTimetable>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IClock clock;

        public CreatePersonalTimetableHandler(IUnitOfWork unitOfWork, IClock clock)
        {
            this.unitOfWork = Ensure.Argument.NotNull(() => unitOfWork);
            this.clock = Ensure.Argument.NotNull(() => clock);
        }

        public async Task HandleAsync(CreatePersonalTimetable command, CancellationToken cancellationToken)
        {
            var timetable = new PersonalTimetable
            {
                UserName = command.UserName,
                InstituteName = command.InstituteName,
                ElementType = command.ElementType,
                ElementName = command.ElementName,
                SchoolYearId = command.SchoolYearId,
                DateCreated = clock.Now
            };

            unitOfWork.Repository<PersonalTimetable>().Add(timetable);
            await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
