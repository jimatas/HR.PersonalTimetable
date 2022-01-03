using Developist.Core.Cqrs.Commands;
using Developist.Core.Persistence;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Api.Infrastructure;
using HR.PersonalCalendar.Api.Models;

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.Api.Commands
{
    [DisplayName("TimetableVisibilityParameters")]
    public class ChangeTimetableVisibility : ICommand
    {
        [Required]
        [JsonPropertyName("id")]
        public Guid PersonalTimetableId { get; set; }

        [Required]
        [JsonPropertyName("visible")]
        public bool IsVisible { get; set; }

        internal string UserNameToVerify { get; set; }
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

        public async Task HandleAsync(ChangeTimetableVisibility parameters, CancellationToken cancellationToken)
        {
            var personalTimetable = await unitOfWork.Repository<PersonalTimetable>().GetAsync(parameters.PersonalTimetableId, cancellationToken).ConfigureAwait(false);
            if (personalTimetable is not null && personalTimetable.VerifyAccess(parameters.UserNameToVerify) && personalTimetable.IsVisible != parameters.IsVisible)
            {
                personalTimetable.IsVisible = parameters.IsVisible;
                personalTimetable.DateLastModified = clock.Now;

                await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
