using Developist.Core.Cqrs;
using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Extensions;
using HR.PersonalTimetable.Api.Infrastructure;
using HR.WebUntisConnector.Extensions;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Api.Queries
{
    public class GetPersonalCalendarsParametersEnsurer : IQueryHandlerWrapper<GetPersonalCalendars, IEnumerable<Models.PersonalCalendar>>
    {
        private readonly IClock clock;

        public GetPersonalCalendarsParametersEnsurer(IClock clock)
        {
            this.clock = Ensure.Argument.NotNull(() => clock);
        }

        public Task<IEnumerable<Models.PersonalCalendar>> HandleAsync(GetPersonalCalendars query, HandlerDelegate<IEnumerable<Models.PersonalCalendar>> next, CancellationToken cancellationToken)
        {
            if (query.StartDate.IsNullOrDefault())
            {
                query.StartDate = clock.Now.Date.GetFirstWeekday();
            }

            if (query.EndDate.IsNullOrDefault())
            {
                query.EndDate = clock.Now.Date.GetLastWeekday().AddDays(1);
            }

            return next();
        }
    }
}
