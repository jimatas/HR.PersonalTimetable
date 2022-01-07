using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Extensions;
using HR.PersonalTimetable.Api.Infrastructure;
using HR.PersonalTimetable.Api.Models;
using HR.PersonalTimetable.Api.Validators;
using HR.WebUntisConnector;
using HR.WebUntisConnector.Configuration;
using HR.WebUntisConnector.Extensions;
using HR.WebUntisConnector.Model;

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
        private readonly IApiClientFactory apiClientFactory;
        private readonly WebUntisConfigurationSection configuration;

        public GetTimetableSchedulesHandler(IQueryDispatcher queryDispatcher, ICachedApiClientFactory apiClientFactory, WebUntisConfigurationSection configuration)
        {
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
            this.apiClientFactory = Ensure.Argument.NotNull(() => apiClientFactory);
            this.configuration = Ensure.Argument.NotNull(() => configuration);
        }

        public async Task<IEnumerable<TimetableSchedule>> HandleAsync(GetTimetableSchedules query, CancellationToken cancellationToken)
        {
            var schedules = new List<TimetableSchedule>();

            var holidaysByInstitute = new Dictionary<string, IEnumerable<Holiday>>(StringComparer.OrdinalIgnoreCase);
            var institutes = configuration.Schools.SelectMany(school => school.Institutes).Select(Institute.FromInstituteElement);

            var personalTimetables = await queryDispatcher.DispatchAsync(new GetPersonalTimetables { UserName = query.UserName }, cancellationToken).ConfigureAwait(false);
            foreach (var personalTimetable in personalTimetables.Where(table => table.IsVisible))
            {
                var schedule = new TimetableSchedule
                {
                    StartDate = (DateTime)query.StartDate,
                    EndDate = (DateTime)query.EndDate,
                    Institute = institutes.FirstOrDefault(institute => institute.Name.Equals(personalTimetable.InstituteName, StringComparison.OrdinalIgnoreCase)),
                    Element = personalTimetable.ToElement()
                };

                schedule.TimetableGroups = await queryDispatcher.DispatchAsync(new GetTimetableGroups
                {
                    InstituteName = personalTimetable.InstituteName,
                    ElementId = schedule.Element.Id,
                    ElementName = schedule.Element.Name,
                    ElementType = schedule.Element.Type,
                    StartDate = schedule.StartDate,
                    EndDate = schedule.EndDate
                }, cancellationToken).ConfigureAwait(false);

                if (!holidaysByInstitute.TryGetValue(personalTimetable.InstituteName, out var holidays))
                {
                    holidays = await GetHolidaysAsync(personalTimetable.InstituteName, schedule.StartDate, schedule.EndDate, cancellationToken).ConfigureAwait(false);
                    holidaysByInstitute.Add(personalTimetable.InstituteName, holidays);
                }
                schedule.Holidays = holidays;

                schedules.Add(schedule);
            }

            return schedules;
        }

        private async Task<IEnumerable<Holiday>> GetHolidaysAsync(string instituteName, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            var apiClient = await apiClientFactory.CreateApiClientAndLogInAsync(instituteName, cancellationToken).ConfigureAwait(false);
            try
            {
                return (await apiClient.GetHolidaysAsync(cancellationToken).ConfigureAwait(false))
                    .Where(holiday => holiday.ToDateTimeRange().Overlaps(new(startDate, endDate)));
            }
            finally
            {
                await apiClient.LogOutAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
