using Developist.Core.Cqrs.Commands;
using Developist.Core.Persistence;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Infrastructure;
using HR.PersonalCalendar.Model;
using HR.WebUntisConnector;
using HR.WebUntisConnector.Extensions;
using HR.WebUntisConnector.Model;

using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.Commands
{
    public class AddPersonalTimetable : ICommand
    {
        public string UserName { get; set; }
        public string InstituteName { get; set; }
        public int ElementId { get; set; }
        public ElementType ElementType { get; set; }
        public string ElementName { get; set; }
    }

    public class AddPersonalTimetableHandler : ICommandHandler<AddPersonalTimetable>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IClock clock;
        private readonly IApiClientFactory apiClientFactory;

        public AddPersonalTimetableHandler(IUnitOfWork unitOfWork, IClock clock, ICachedApiClientFactory apiClientFactory)
        {
            this.unitOfWork = Ensure.Argument.NotNull(() => unitOfWork);
            this.clock = Ensure.Argument.NotNull(() => clock);
            this.apiClientFactory = Ensure.Argument.NotNull(() => apiClientFactory);
        }

        public async Task HandleAsync(AddPersonalTimetable command, CancellationToken cancellationToken)
        {
            var personalTimetable = new PersonalTimetable
            {
                UserName = command.UserName,
                InstituteName = command.InstituteName,
                ElementType = command.ElementType,
                ElementName = command.ElementName,
                ElementId = command.ElementId,
                SchoolYearId = (await LookupSchoolYearAsync(command.InstituteName, cancellationToken).ConfigureAwait(false))?.Id,
                DateCreated = clock.Now
            };

            unitOfWork.Repository<PersonalTimetable>().Add(personalTimetable);
            await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Looks up the current school year from WebUntis.
        /// </summary>
        /// <param name="instituteName">The RUAS institute under which the school year is defined.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<SchoolYear> LookupSchoolYearAsync(string instituteName, CancellationToken cancellationToken)
        {
            var apiClient = await apiClientFactory.CreateApiClientAndLogInAsync(instituteName, cancellationToken).ConfigureAwait(false);
            try
            {
                return await apiClient.GetCurrentSchoolYearAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await apiClient.LogOutAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
