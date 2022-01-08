using Developist.Core.Cqrs;
using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Extensions;
using HR.PersonalTimetable.Api.Infrastructure;
using HR.PersonalTimetable.Api.Models;
using HR.WebUntisConnector.Extensions;
using HR.WebUntisConnector.Model;

using Microsoft.Extensions.Options;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Api.Queries
{
    public class GetTimetableScheduleParametersEnsurer : IQueryHandlerWrapper<GetTimetableSchedule, TimetableSchedule>
    {
        private readonly IQueryDispatcher queryDispatcher;
        private readonly IClock clock;
        private readonly AppSettings appSettings;

        public GetTimetableScheduleParametersEnsurer(IQueryDispatcher queryDispatcher, IClock clock, IOptions<AppSettings> appSettings)
        {
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
            this.clock = Ensure.Argument.NotNull(() => clock);
            this.appSettings = Ensure.Argument.NotNull(() => appSettings).Value;
        }

        public async Task<TimetableSchedule> HandleAsync(GetTimetableSchedule query, HandlerDelegate<TimetableSchedule> next, CancellationToken cancellationToken)
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

            if (new DateTimeRange((DateTime)query.StartDate, (DateTime)query.EndDate).Duration.TotalDays > appSettings.MaxTimetableRangeInDays)
            {
                throw new BadRequestException($"Requested timetable range too large. Maximum of {appSettings.MaxTimetableRangeInDays} days allowed.");
            }

            return await next().ConfigureAwait(false);
        }
    }
}
