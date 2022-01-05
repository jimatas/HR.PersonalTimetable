using Developist.Core.Cqrs;
using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Api.Extensions;
using HR.PersonalCalendar.Api.Infrastructure;
using HR.PersonalCalendar.Api.Models;
using HR.WebUntisConnector.Extensions;
using HR.WebUntisConnector.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.Api.Queries
{
    public class GetTimetableGroupsParametersEnsurer : IQueryHandlerWrapper<GetTimetableGroups, IEnumerable<TimetableGroup>>
    {
        private readonly IQueryDispatcher queryDispatcher;
        private readonly IClock clock;

        public GetTimetableGroupsParametersEnsurer(IQueryDispatcher queryDispatcher, IClock clock)
        {
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
            this.clock = Ensure.Argument.NotNull(() => clock);
        }

        public async Task<IEnumerable<TimetableGroup>> HandleAsync(GetTimetableGroups query, HandlerDelegate<IEnumerable<TimetableGroup>> next, CancellationToken cancellationToken)
        {
            if (query.ElementId.IsNullOrDefault() || string.IsNullOrEmpty(query.ElementName))
            {
                var elements = await queryDispatcher.DispatchAsync(new GetElements
                {
                    InstituteName = query.InstituteName,
                    ElementType = query.ElementType
                }, cancellationToken).ConfigureAwait(false);

                var element = elements.FirstOrDefault(e => e.Id == query.ElementId || e.Name.Equals(query.ElementName, StringComparison.OrdinalIgnoreCase));
                if (element is null)
                {
                    throw query.ElementId.IsNullOrDefault()
                        ? new NotFoundException($"No {query.ElementType} with {nameof(Element.Name)} {query.ElementName} found.")
                        : new NotFoundException($"No {query.ElementType} with {nameof(Element.Id)} {query.ElementId} found.");
                }

                query.ElementId = element.Id;
                query.ElementName = element.Name;
            }

            if (query.StartDate.IsNullOrDefault())
            {
                query.StartDate = clock.Now.Date.GetFirstWeekday();
            }

            if (query.EndDate.IsNullOrDefault())
            {
                query.EndDate = clock.Now.Date.GetLastWeekday().AddDays(1);
            }

            return await next().ConfigureAwait(false);
        }
    }
}
