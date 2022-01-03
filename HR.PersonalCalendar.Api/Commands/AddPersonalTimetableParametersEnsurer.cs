using Developist.Core.Cqrs;
using Developist.Core.Cqrs.Commands;
using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Api.Extensions;
using HR.PersonalCalendar.Api.Models;
using HR.PersonalCalendar.Api.Queries;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.Api.Commands
{
    public class AddPersonalTimetableParametersEnsurer : ICommandHandlerWrapper<AddPersonalTimetable>
    {
        private readonly IQueryDispatcher queryDispatcher;

        public AddPersonalTimetableParametersEnsurer(IQueryDispatcher queryDispatcher)
        {
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
        }

        public async Task HandleAsync(AddPersonalTimetable parameters, HandlerDelegate next, CancellationToken cancellationToken)
        {
            if (parameters.ElementId.IsNullOrDefault() || string.IsNullOrEmpty(parameters.ElementName))
            {
                var elements = await queryDispatcher.DispatchAsync(new GetElements
                {
                    InstituteName = parameters.InstituteName,
                    ElementType = parameters.ElementType
                }, cancellationToken).ConfigureAwait(false);

                var element = elements.FirstOrDefault(e => e.Id == parameters.ElementId || e.Name.Equals(parameters.ElementName, StringComparison.InvariantCultureIgnoreCase));
                if (element is null)
                {
                    throw parameters.ElementId.IsNullOrDefault() switch
                    {
                        true => new NoSuchElementException(parameters.ElementType, parameters.ElementName),
                        false => new NoSuchElementException(parameters.ElementType, (int)parameters.ElementId),
                    };
                }

                parameters.ElementId = element.Id;
                parameters.ElementName = element.Name;
            }

            await next().ConfigureAwait(false);
        }
    }
}
