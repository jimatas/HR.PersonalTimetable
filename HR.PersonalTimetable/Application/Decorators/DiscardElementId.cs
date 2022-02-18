﻿using Developist.Core.Cqrs;
using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Application.Models;
using HR.PersonalTimetable.Application.Queries;

using Microsoft.Extensions.Options;

using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Application.Decorators
{
    public class DiscardElementId : IPrioritizable, IQueryHandlerWrapper<GetSchedule, Schedule>
    {
        private readonly AppSettings appSettings;

        public DiscardElementId(IOptions<AppSettings> appSettings)
        {
            this.appSettings = Ensure.Argument.NotNull(() => appSettings).Value;
        }

        public sbyte Priority => Priorities.High; // Needs to run before EnsureElementIdAndName, which has 'Normal' priority.

        public Task<Schedule> HandleAsync(GetSchedule query, HandlerDelegate<Schedule> next, CancellationToken cancellationToken)
        {
            if (!query.ElementId.IsNullOrDefault() && !string.IsNullOrEmpty(query.ElementName) && appSettings.AlwaysLookUpElementId)
            {
                // If the element's ID was specified in the API call, discard it so that the next decorator in the pipeline (which ensures both the element's ID and name are set)
                // will look up the missing ID from the correct WebUntis environment before ultimately passing along the query to the handler.

                query.ElementId = null;
            }

            return next();
        }
    }
}