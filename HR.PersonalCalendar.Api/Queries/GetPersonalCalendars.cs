using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Api.Extensions;
using HR.PersonalCalendar.Api.Infrastructure;
using HR.PersonalCalendar.Api.Models;
using HR.PersonalCalendar.Api.Validators;
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

namespace HR.PersonalCalendar.Api.Queries
{
    public class GetPersonalCalendars : IQuery<IEnumerable<Models.PersonalCalendar>>
    {
        [Required, UserName]
        [FromRoute(Name = "user")]
        public string UserName { get; set; }

        [FromQuery(Name = "start")]
        public DateTime? StartDate { get; set; }

        [FromQuery(Name = "end")]
        public DateTime? EndDate { get; set; }
    }

    public class GetPersonalCalendarsHandler : IQueryHandler<GetPersonalCalendars, IEnumerable<Models.PersonalCalendar>>
    {
        private readonly IQueryDispatcher queryDispatcher;
        private readonly IApiClientFactory apiClientFactory;
        private readonly WebUntisConfigurationSection configuration;

        public GetPersonalCalendarsHandler(IQueryDispatcher queryDispatcher, ICachedApiClientFactory apiClientFactory, WebUntisConfigurationSection configuration)
        {
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
            this.apiClientFactory = Ensure.Argument.NotNull(() => apiClientFactory);
            this.configuration = Ensure.Argument.NotNull(() => configuration);
        }

        public async Task<IEnumerable<Models.PersonalCalendar>> HandleAsync(GetPersonalCalendars query, CancellationToken cancellationToken)
        {
            var calendars = new List<Models.PersonalCalendar>();

            var holidaysByInstitute = new Dictionary<string, IEnumerable<Holiday>>(StringComparer.InvariantCultureIgnoreCase);
            var institutes = configuration.Schools.SelectMany(school => school.Institutes).Select(Institute.FromInstituteElement);

            var personalTimetables = await queryDispatcher.DispatchAsync(new GetPersonalTimetables { UserName = query.UserName }, cancellationToken).ConfigureAwait(false);
            foreach (var personalTimetable in personalTimetables.Where(table => table.IsVisible))
            {
                var calendar = new Models.PersonalCalendar
                {
                    StartDate = (DateTime)query.StartDate,
                    EndDate = (DateTime)query.EndDate,
                    Institute = institutes.FirstOrDefault(institute => institute.Name.Equals(personalTimetable.InstituteName, StringComparison.InvariantCultureIgnoreCase)),
                    Element = personalTimetable.ToElement()
                };

                calendar.TimetableGroups = await queryDispatcher.DispatchAsync(new GetTimetableGroups
                {
                    InstituteName = personalTimetable.InstituteName,
                    ElementId = calendar.Element.Id,
                    ElementName = calendar.Element.Name,
                    ElementType = calendar.Element.Type,
                    StartDate = calendar.StartDate,
                    EndDate = calendar.EndDate
                }, cancellationToken).ConfigureAwait(false);

                if (!holidaysByInstitute.TryGetValue(personalTimetable.InstituteName, out var holidays))
                {
                    holidays = await GetHolidaysAsync(personalTimetable.InstituteName, calendar.StartDate, calendar.EndDate, cancellationToken).ConfigureAwait(false);
                    holidaysByInstitute.Add(personalTimetable.InstituteName, holidays);
                }
                calendar.Holidays = holidays;

                calendars.Add(calendar);
            }

            return calendars;
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
