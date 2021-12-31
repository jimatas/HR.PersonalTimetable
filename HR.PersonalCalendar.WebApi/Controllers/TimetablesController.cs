using Developist.Core.Cqrs;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Infrastructure;
using HR.PersonalCalendar.Queries;
using HR.PersonalCalendar.WebApi.Extensions;
using HR.PersonalCalendar.WebApi.Models;
using HR.WebUntisConnector.Configuration;
using HR.WebUntisConnector.Extensions;
using HR.WebUntisConnector.Model;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.WebApi.Controllers
{
    public class TimetablesController : ApiControllerBase
    {
        private readonly IClock clock;
        private readonly WebUntisConfigurationSection configuration;

        public TimetablesController(
            IClock clock,
            WebUntisConfigurationSection webuntisConfiguration,
            IWebHostEnvironment environment,
            IConfiguration configuration,
            IDispatcher dispatcher) : base(environment, configuration, dispatcher)
        {
            this.clock = Ensure.Argument.NotNull(() => clock);
            this.configuration = Ensure.Argument.NotNull(() => webuntisConfiguration);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TimetableGroup>>> GetAsync(
            [FromQuery(Name = "institute"), BindRequired] string instituteName,
            [FromQuery(Name = "element"), BindRequired] ElementType elementType,
            [FromQuery(Name = "name"), BindRequired] string elementName,
            [FromQuery(Name = "start")] DateTime? startDate,
            [FromQuery(Name = "end")] DateTime? endDate,
            CancellationToken cancellationToken = default)
        {
            var elements = await QueryDispatcher.DispatchAsync(new GetElementsByType { InstituteName = instituteName, ElementType = elementType }, cancellationToken);

            var element = elements.FirstOrDefault(e => e.Name.Equals(elementName, StringComparison.InvariantCultureIgnoreCase));
            if (element is null)
            {
                return NotFound();
            }

            var timetableGroups = await QueryDispatcher.DispatchAsync(new GetTimetableGroups
            {
                InstituteName = instituteName,
                Element = element,
                StartDate = startDate ?? clock.Now.Date.GetFirstWeekday(),
                EndDate = endDate ?? clock.Now.Date.GetLastWeekday().AddDays(1)
            }, cancellationToken);

            return Ok(timetableGroups);
        }

        [HttpGet("personalized")]
        public async Task<ActionResult<IEnumerable<PersonalCalendarModel>>> GetPersonalizedAsync(
            [FromQuery(Name = "user"), BindRequired] string userName,
            [FromQuery(Name = "start")] DateTime? startDate,
            [FromQuery(Name = "end")] DateTime? endDate,
            CancellationToken cancellationToken = default)
        {
            var calendarModels = new List<PersonalCalendarModel>();

            var holidaysByInstitute = new Dictionary<string, IEnumerable<Holiday>>(StringComparer.InvariantCultureIgnoreCase);
            var instituteModels = configuration.Schools.SelectMany(school => school.Institutes).Select(InstituteModel.FromInstituteElement);

            var personalTimetables = await QueryDispatcher.DispatchAsync(new GetPersonalTimetables { UserName = userName }, cancellationToken);
            foreach (var personalTimetable in personalTimetables)
            {
                var calendarModel = new PersonalCalendarModel
                {
                    StartDate = startDate ?? clock.Now.Date.GetFirstWeekday(),
                    EndDate = endDate ?? clock.Now.Date.GetLastWeekday().AddDays(1),
                    Institute = instituteModels.FirstOrDefault(institute => institute.Name.Equals(personalTimetable.InstituteName, StringComparison.InvariantCultureIgnoreCase))
                };

                calendarModel.TimetableGroups = await QueryDispatcher.DispatchAsync(new GetTimetableGroups
                {
                    InstituteName = personalTimetable.InstituteName,
                    Element = PersonalizationModel.FromPersonalTimetable(personalTimetable).ToElement(),
                    StartDate = calendarModel.StartDate,
                    EndDate = calendarModel.EndDate
                }, cancellationToken);

                if (!holidaysByInstitute.ContainsKey(personalTimetable.InstituteName))
                {
                    var holidays = await QueryDispatcher.DispatchAsync(new GetHolidays
                    {
                        InstituteName = personalTimetable.InstituteName,
                        StartDate = calendarModel.StartDate,
                        EndDate = calendarModel.EndDate
                    }, cancellationToken);

                    holidaysByInstitute.Add(personalTimetable.InstituteName, holidays);
                }

                calendarModel.Holidays = holidaysByInstitute[personalTimetable.InstituteName];
                calendarModels.Add(calendarModel);
            }

            return calendarModels;
        }
    }
}
