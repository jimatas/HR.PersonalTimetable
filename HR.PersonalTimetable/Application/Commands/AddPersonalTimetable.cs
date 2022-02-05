using Developist.Core.Cqrs.Commands;
using Developist.Core.Persistence;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Application.Services;
using HR.PersonalTimetable.Application.Validators;
using HR.WebUntisConnector;
using HR.WebUntisConnector.Extensions;
using HR.WebUntisConnector.Model;

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Application.Commands
{
    [DisplayName("PersonalTimetable.Params")]
    public class AddPersonalTimetable : AuthorizableCommandBase, IValidatableObject
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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ElementId.IsNullOrDefault() && string.IsNullOrEmpty(ElementName))
            {
                var errorMessage = $"Either the {nameof(ElementId)} field or the {nameof(ElementName)} field, or both must be specified.";
                var memberNames = new[] { nameof(ElementId), nameof(ElementName) };
                return new ValidationResult[] { new(errorMessage, memberNames) };
            }
            return Enumerable.Empty<ValidationResult>();
        }
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
            Models.PersonalTimetable personalTimetable = new()
            {
                Integration = command.Authorization.SigningKey.Integration,
                UserName = command.UserName,
                InstituteName = command.InstituteName,
                ElementType = command.ElementType,
                ElementName = command.ElementName,
                ElementId = (int)command.ElementId,
                SchoolYearId = (await GetSchoolYearAsync(command.InstituteName, cancellationToken).WithoutCapturingContext())?.Id,
                DateCreated = clock.Now
            };

            personalTimetable.VerifyCreateAccess(command.Authorization);
            
            unitOfWork.Repository<Models.PersonalTimetable>().Add(personalTimetable);
            await unitOfWork.CompleteAsync(cancellationToken).WithoutCapturingContext();
        }

        private async Task<SchoolYear> GetSchoolYearAsync(string instituteName, CancellationToken cancellationToken)
        {
            var apiClient = await apiClientFactory.CreateApiClientAndLogInAsync(instituteName, cancellationToken).WithoutCapturingContext();
            try
            {
                return await apiClient.GetCurrentSchoolYearAsync(cancellationToken).WithoutCapturingContext();
            }
            finally
            {
                await apiClient.LogOutAsync(cancellationToken).WithoutCapturingContext();
            }
        }
    }
}
