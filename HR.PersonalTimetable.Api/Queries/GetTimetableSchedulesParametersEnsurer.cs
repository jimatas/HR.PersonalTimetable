using Developist.Core.Cqrs;
using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Extensions;
using HR.PersonalTimetable.Api.Infrastructure;
using HR.PersonalTimetable.Api.Models;
using HR.WebUntisConnector.Extensions;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Api.Queries
{
    public class GetTimetableSchedulesParametersEnsurer : IQueryHandlerWrapper<GetTimetableSchedules, IEnumerable<TimetableSchedule>>
    {
        private readonly IClock clock;

        public GetTimetableSchedulesParametersEnsurer(IClock clock)
        {
            this.clock = Ensure.Argument.NotNull(() => clock);
        }

        public Task<IEnumerable<TimetableSchedule>> HandleAsync(GetTimetableSchedules query, HandlerDelegate<IEnumerable<TimetableSchedule>> next, CancellationToken cancellationToken)
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
