using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Application.Models;
using HR.PersonalTimetable.Application.Validators;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Application.Queries
{
    public class GetSchedules : IQuery<IEnumerable<Schedule>>
    {
        /// <summary>
        /// The username of the user. 
        /// Depending on their role, either an employee code or a student number.
        /// </summary>
        [Required, UserName]
        [FromRoute(Name = "user")]
        public string UserName { get; set; }

        /// <summary>
        /// Optional start of the date range.
        /// Defaults to the first weekday of the current week.
        /// </summary>
        [FromQuery(Name = "start")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Optional end of the date range.
        /// Defaults to the last weekday (+1 day) of the current week.
        /// </summary>
        [FromQuery(Name = "end")]
        public DateTime? EndDate { get; set; }
    }

    public class GetSchedulesHandler : IQueryHandler<GetSchedules, IEnumerable<Schedule>>
    {
        private readonly IQueryDispatcher queryDispatcher;
        
        public GetSchedulesHandler(IQueryDispatcher queryDispatcher)
        {
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
        }

        public async Task<IEnumerable<Schedule>> HandleAsync(GetSchedules query, CancellationToken cancellationToken)
        {
            var schedules = new List<Schedule>();

            var personalTimetables = await queryDispatcher.DispatchAsync(new GetPersonalTimetables { UserName = query.UserName }, cancellationToken).ConfigureAwait(false);
            foreach (var personalTimetable in personalTimetables.Where(table => table.IsVisible))
            {
                var schedule = await queryDispatcher.DispatchAsync(new GetSchedule
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
