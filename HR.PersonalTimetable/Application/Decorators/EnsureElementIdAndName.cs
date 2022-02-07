using Developist.Core.Cqrs;
using Developist.Core.Cqrs.Commands;
using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Application.Commands;
using HR.PersonalTimetable.Application.Exceptions;
using HR.PersonalTimetable.Application.Models;
using HR.PersonalTimetable.Application.Queries;
using HR.WebUntisConnector.Model;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Application.Decorators
{
    public class EnsureElementIdAndName : ICommandHandlerWrapper<AddPersonalTimetable>, IQueryHandlerWrapper<GetSchedule, Schedule>
    {
        private readonly IQueryDispatcher queryDispatcher;

        public EnsureElementIdAndName(IQueryDispatcher queryDispatcher)
        {
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
        }

        public async Task HandleAsync(AddPersonalTimetable command, HandlerDelegate next, CancellationToken cancellationToken)
        {
            (command.ElementId, command.ElementName) = await GetElementIdAndNameAsync(
                command.InstituteName,
                command.ElementType,
                command.ElementId,
                command.ElementName, cancellationToken);

            await next();
        }

        public async Task<Schedule> HandleAsync(GetSchedule query, HandlerDelegate<Schedule> next, CancellationToken cancellationToken)
        {
            (query.ElementId, query.ElementName) = await GetElementIdAndNameAsync(
                query.InstituteName,
                query.ElementType,
                query.ElementId,
                query.ElementName, cancellationToken);

            return await next();
        }

        private async Task<(int, string)> GetElementIdAndNameAsync(string instituteName, ElementType elementType, int? elementId, string elementName, CancellationToken cancellationToken)
        {
            if (elementId.IsNullOrDefault() || string.IsNullOrEmpty(elementName))
            {
                var elements = await queryDispatcher.DispatchAsync(new GetElements
                {
                    InstituteName = instituteName,
                    ElementType = elementType
                }, cancellationToken);

                var element = elements.FirstOrDefault(e => e.Id == elementId || e.Name.Equals(elementName, StringComparison.OrdinalIgnoreCase));
                if (element is null)
                {
                    throw elementId.IsNullOrDefault()
                        ? new NotFoundException($"No {elementType} with {nameof(Element.Name)} \"{elementName}\" found.")
                        : new NotFoundException($"No {elementType} with {nameof(Element.Id)} {elementId} found.");
                }

                elementId = element.Id;
                elementName = element.Name;
            }

            return ((int)elementId, elementName);
        }
    }
}
