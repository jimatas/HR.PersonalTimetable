using Developist.Core.Cqrs.Commands;
using Developist.Core.Cqrs.Queries;
using Developist.Core.Persistence;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Infrastructure;
using HR.PersonalCalendar.Model;
using HR.PersonalCalendar.Queries;
using HR.WebUntisConnector;
using HR.WebUntisConnector.Extensions;
using HR.WebUntisConnector.Model;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.Commands
{
    public class CreatePersonalTimetable : ICommand
    {
        public string UserName { get; set; }
        public string InstituteName { get; set; }
        public ElementType ElementType { get; set; }
        public string ElementName { get; set; }
    }

    public class CreatePersonalTimetableHandler : ICommandHandler<CreatePersonalTimetable>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IClock clock;
        private readonly IQueryDispatcher queryDispatcher;
        private readonly IApiClientFactory apiClientFactory;

        public CreatePersonalTimetableHandler(IUnitOfWork unitOfWork, IClock clock, IQueryDispatcher queryDispatcher, ICachedApiClientFactory apiClientFactory)
        {
            this.unitOfWork = Ensure.Argument.NotNull(() => unitOfWork);
            this.clock = Ensure.Argument.NotNull(() => clock);
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
            this.apiClientFactory = Ensure.Argument.NotNull(() => apiClientFactory);
        }

        public async Task HandleAsync(CreatePersonalTimetable command, CancellationToken cancellationToken)
        {
            var timetable = new PersonalTimetable
            {
                UserName = command.UserName,
                InstituteName = command.InstituteName,
                ElementType = command.ElementType,
                ElementName = command.ElementName,
                ElementId = (await LookupElementAsync(command.InstituteName, command.ElementType, command.ElementName, cancellationToken).ConfigureAwait(false))?.Id,
                SchoolYearId = (await LookupSchoolYearAsync(command.InstituteName, cancellationToken).ConfigureAwait(false))?.Id,
                DateCreated = clock.Now
            };

            unitOfWork.Repository<PersonalTimetable>().Add(timetable);
            await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Looks up an <see cref="Element"/> object given its type and name.
        /// </summary>
        /// <param name="instituteName">The RUAS institute under which the element is defined.</param>
        /// <param name="elementType"></param>
        /// <param name="elementName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<Element> LookupElementAsync(string instituteName, ElementType elementType, string elementName, CancellationToken cancellationToken)
        {
            var elements = await queryDispatcher.DispatchAsync(new GetElementsByType
            {
                InstituteName = instituteName,
                ElementType = elementType
            }, cancellationToken).ConfigureAwait(false);

            return elements.FirstOrDefault(e => e.Name.Equals(elementName, StringComparison.InvariantCultureIgnoreCase));
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
