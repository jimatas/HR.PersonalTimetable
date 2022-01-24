using Developist.Core.Cqrs.Commands;
using Developist.Core.Persistence;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Application.Extensions;
using HR.PersonalTimetable.Application.Models;
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
    public class AddPersonalTimetable : ICommand, IValidatableObject
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

        /// <summary>
        /// The integration through which the timetable is being created.
        /// </summary>
        internal Integration Integration { get; set; }

        /// <summary>
        /// A salted hash of the username to verify.
        /// The salt is the (current) signing key of the integration through which the timetable is being created.
        /// </summary>
        internal string UserNameToVerify { get; set; }

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
                Integration = command.Integration,
                UserName = command.UserName,
                InstituteName = command.InstituteName,
                ElementType = command.ElementType,
                ElementName = command.ElementName,
                ElementId = (int)command.ElementId,
                SchoolYearId = (await GetSchoolYearAsync(command.InstituteName, cancellationToken).ConfigureAwait(false))?.Id,
                DateCreated = clock.Now
            };

            personalTimetable.VerifyCreateAccess(command.UserNameToVerify, command.Integration.CurrentSigningKey);
            
            unitOfWork.Repository<Models.PersonalTimetable>().Add(personalTimetable);
            await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<SchoolYear> GetSchoolYearAsync(string instituteName, CancellationToken cancellationToken)
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
