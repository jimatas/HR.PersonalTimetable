using Developist.Core.Cqrs.Commands;
using Developist.Core.Cqrs.Queries;
using Developist.Core.Persistence;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Extensions;
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
    public class AddPersonalTimetable : ICommand
    {
        public string UserName { get; set; }
        public string InstituteName { get; set; }
        public ElementType ElementType { get; set; }
        public int? ElementId { get; set; }
        public string ElementName { get; set; }
    }

    public class AddPersonalTimetableHandler : ICommandHandler<AddPersonalTimetable>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IClock clock;
        private readonly IApiClientFactory apiClientFactory;
        private readonly IQueryDispatcher queryDispatcher;

        public AddPersonalTimetableHandler(IUnitOfWork unitOfWork, IClock clock, ICachedApiClientFactory apiClientFactory, IQueryDispatcher queryDispatcher)
        {
            this.unitOfWork = Ensure.Argument.NotNull(() => unitOfWork);
            this.clock = Ensure.Argument.NotNull(() => clock);
            this.apiClientFactory = Ensure.Argument.NotNull(() => apiClientFactory);
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
        }

        public async Task HandleAsync(AddPersonalTimetable command, CancellationToken cancellationToken)
        {
            await EnsureElementIdAndElementNameAsync(command, cancellationToken).ConfigureAwait(false);

            var personalTimetable = new PersonalTimetable
            {
                UserName = command.UserName,
                InstituteName = command.InstituteName,
                ElementType = command.ElementType,
                ElementName = command.ElementName,
                ElementId = (int)command.ElementId,
                SchoolYearId = (await LookupSchoolYearAsync(command.InstituteName, cancellationToken).ConfigureAwait(false))?.Id,
                DateCreated = clock.Now
            };

            unitOfWork.Repository<PersonalTimetable>().Add(personalTimetable);
            await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Ensures that both the <see cref="AddPersonalTimetable.ElementId"/> and the <see cref="AddPersonalTimetable.ElementName"/>  properties of the specified <see cref="AddPersonalTimetable"/> command have their values set.
        /// Depending on whether one is missing, looks up one or the other using the <see cref="GetElementsByType"/> query to retrieve all applicable elements from WebUntis.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task EnsureElementIdAndElementNameAsync(AddPersonalTimetable command, CancellationToken cancellationToken)
        {
            if (command.ElementId.IsNullOrDefault() || string.IsNullOrEmpty(command.ElementName))
            {
                var elements = await queryDispatcher.DispatchAsync(new GetElementsByType
                {
                    InstituteName = command.InstituteName,
                    ElementType = command.ElementType
                }, cancellationToken).ConfigureAwait(false);

                var element = elements.FirstOrDefault(e => e.Id == command.ElementId || e.Name.Equals(command.ElementName, StringComparison.InvariantCultureIgnoreCase));
                if (element is null)
                {
                    throw command.ElementId.IsNullOrDefault() switch
                    {
                        true => new NoSuchElementException(command.ElementType, command.ElementName),
                        false => new NoSuchElementException(command.ElementType, (int)command.ElementId),
                    };
                }

                command.ElementId = element.Id;
                command.ElementName = element.Name;
            }
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
