using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Models;
using HR.PersonalTimetable.Api.Validators;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Api.Queries
{
    public class GetTimetableSchedules : IQuery<IEnumerable<TimetableSchedule>>
    {
        [Required, UserName]
        [FromRoute(Name = "user")]
        public string UserName { get; set; }

        [FromQuery(Name = "start")]
        public DateTime? StartDate { get; set; }

        [FromQuery(Name = "end")]
        public DateTime? EndDate { get; set; }
    }

    public class GetTimetableSchedulesHandler : IQueryHandler<GetTimetableSchedules, IEnumerable<TimetableSchedule>>
    {
        private readonly IQueryDispatcher queryDispatcher;
        
        public GetTimetableSchedulesHandler(IQueryDispatcher queryDispatcher)
        {
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
        }

        public async Task<IEnumerable<TimetableSchedule>> HandleAsync(GetTimetableSchedules query, CancellationToken cancellationToken)
        {
            var schedules = new List<TimetableSchedule>();

            var personalTimetables = await queryDispatcher.DispatchAsync(new GetPersonalTimetables { UserName = query.UserName }, cancellationToken).ConfigureAwait(false);
            foreach (var personalTimetable in personalTimetables.Where(table => table.IsVisible))
            {
                var schedule = await queryDispatcher.DispatchAsync(new GetTimetableSchedule
                {
                    InstituteName = personalTimetable.InstituteName,
                    ElementType = personalTimetable.ElementType,
                    ElementId = personalTimetable.ElementId,
                    ElementName = personalTimetable.ElementName,
                    StartDate = query.StartDate,
                    EndDate = query.EndDate
                }, cancellationToken).ConfigureAwait(false);

                schedules.Add(schedule);
            }

            return schedules;
        }
    }
}
