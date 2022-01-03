using Developist.Core.Cqrs.Commands;
using Developist.Core.Persistence;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Api.Infrastructure;
using HR.PersonalCalendar.Api.Models;
using HR.PersonalCalendar.Api.Validators;
using HR.WebUntisConnector;
using HR.WebUntisConnector.Extensions;
using HR.WebUntisConnector.Model;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.Api.Commands
{
    [DisplayName("PersonalTimetableParameters")]
    public class AddPersonalTimetable : ICommand
    {
        [Required, UserName]
        public string UserName { get; set; }

        [Required, StringLength(50)]
        public string InstituteName { get; set; }

        [Required]
        public ElementType ElementType { get; set; }

        public int? ElementId { get; set; }

        [StringLength(100)]
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

        public async Task HandleAsync(AddPersonalTimetable parameters, CancellationToken cancellationToken)
        {
            PersonalTimetable entity = new()
            {
                UserName = parameters.UserName,
                InstituteName = parameters.InstituteName,
                ElementType = parameters.ElementType,
                ElementName = parameters.ElementName,
                ElementId = (int)parameters.ElementId,
                SchoolYearId = (await LookupSchoolYearAsync(parameters.InstituteName, cancellationToken).ConfigureAwait(false))?.Id,
                DateCreated = clock.Now
            };

            unitOfWork.Repository<PersonalTimetable>().Add(entity);
            await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);
        }

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
