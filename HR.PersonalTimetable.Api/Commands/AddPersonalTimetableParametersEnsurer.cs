﻿using Developist.Core.Cqrs;
using Developist.Core.Cqrs.Commands;
using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Extensions;
using HR.PersonalTimetable.Api.Models;
using HR.PersonalTimetable.Api.Queries;
using HR.WebUntisConnector.Model;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Api.Commands
{
    public class AddPersonalTimetableParametersEnsurer : ICommandHandlerWrapper<AddPersonalTimetable>
    {
        private readonly IQueryDispatcher queryDispatcher;

        public AddPersonalTimetableParametersEnsurer(IQueryDispatcher queryDispatcher)
        {
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
        }

        public async Task HandleAsync(AddPersonalTimetable command, HandlerDelegate next, CancellationToken cancellationToken)
        {
            if (command.ElementId.IsNullOrDefault() || string.IsNullOrEmpty(command.ElementName))
            {
                var elements = await queryDispatcher.DispatchAsync(new GetElements
                {
                    InstituteName = command.InstituteName,
                    ElementType = command.ElementType
                }, cancellationToken).ConfigureAwait(false);

                var element = elements.FirstOrDefault(e => e.Id == command.ElementId || e.Name.Equals(command.ElementName, StringComparison.OrdinalIgnoreCase));
                if (element is null)
                {
                    throw command.ElementId.IsNullOrDefault()
                        ? new NotFoundException($"No {command.ElementType} with {nameof(Element.Name)} {command.ElementName} found.")
                        : new NotFoundException($"No {command.ElementType} with {nameof(Element.Id)} {command.ElementId} found.");
                }

                command.ElementId = element.Id;
                command.ElementName = element.Name;
            }

            await next().ConfigureAwait(false);
        }
    }
}