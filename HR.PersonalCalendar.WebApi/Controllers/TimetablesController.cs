using Developist.Core.Cqrs;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Extensions;
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
using System.Text;
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
            [FromQuery(Name = "id")] int? elementId,
            [FromQuery(Name = "name")] string elementName,
            [FromQuery(Name = "start")] DateTime? startDate,
            [FromQuery(Name = "end")] DateTime? endDate,
            CancellationToken cancellationToken = default)
        {
            if (elementId.IsNullOrDefault() && string.IsNullOrEmpty(elementName))
            {
                ModelState.AddModelError("id", "Either the element's id or its name must be specified.");
                ModelState.AddModelError("name", "Either the element's id or its name must be specified.");
                return BadRequest(ModelState);
            }

            var elements = await QueryDispatcher.DispatchAsync(new GetElementsByType { InstituteName = instituteName, ElementType = elementType }, cancellationToken);

            var element = elements.FirstOrDefault(e => e.Id == elementId || e.Name.Equals(elementName, StringComparison.InvariantCultureIgnoreCase));
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
        public async Task<IEnumerable<Models.PersonalCalendar>> GetPersonalizedAsync(
            [FromQuery(Name = "user"), BindRequired] string userName,
            [FromQuery(Name = "start")] DateTime? startDate,
            [FromQuery(Name = "end")] DateTime? endDate,
            CancellationToken cancellationToken = default)
        {
            var calendars = new List<Models.PersonalCalendar>();

            var holidaysByInstitute = new Dictionary<string, IEnumerable<Holiday>>(StringComparer.InvariantCultureIgnoreCase);
            var institutes = configuration.Schools.SelectMany(school => school.Institutes).Select(Institute.FromInstituteElement);

            var personalTimetables = await QueryDispatcher.DispatchAsync(new GetPersonalTimetables { UserName = userName }, cancellationToken);
            foreach (var personalTimetable in personalTimetables.Where(table => table.IsVisible))
            {
                var calendar = new Models.PersonalCalendar
                {
                    StartDate = startDate ?? clock.Now.Date.GetFirstWeekday(),
                    EndDate = endDate ?? clock.Now.Date.GetLastWeekday().AddDays(1),
                    Institute = institutes.FirstOrDefault(institute => institute.Name.Equals(personalTimetable.InstituteName, StringComparison.InvariantCultureIgnoreCase)),
                    Element = Personalization.FromPersonalTimetable(personalTimetable).ToElement()
                };

                calendar.TimetableGroups = await QueryDispatcher.DispatchAsync(new GetTimetableGroups
                {
                    InstituteName = personalTimetable.InstituteName,
                    Element = calendar.Element,
                    StartDate = calendar.StartDate,
                    EndDate = calendar.EndDate
                }, cancellationToken);

                if (!holidaysByInstitute.TryGetValue(personalTimetable.InstituteName, out var holidays))
                {
                    holidays = await QueryDispatcher.DispatchAsync(new GetHolidays
                    {
                        InstituteName = personalTimetable.InstituteName,
                        StartDate = calendar.StartDate,
                        EndDate = calendar.EndDate
                    }, cancellationToken);

                    holidaysByInstitute.Add(personalTimetable.InstituteName, holidays);
                }
                calendar.Holidays = holidays;

                calendars.Add(calendar);
            }

            return calendars;
        }

        [HttpGet("personalized/export")]
        public async Task<ActionResult> GetPersonalizedExportAsync(
            [FromQuery(Name = "user"), BindRequired] string userName,
            [FromQuery(Name = "start")] DateTime? startDate,
            [FromQuery(Name = "end")] DateTime? endDate,
            CancellationToken cancellationToken = default)
        {
            var calendars = await GetPersonalizedAsync(userName, startDate, endDate, cancellationToken);
            var exportedData = calendars.SelectMany(calendar => calendar.TimetableGroups).ExportCalendar();
            return File(Encoding.UTF8.GetBytes(exportedData), contentType: "text/calendar", fileDownloadName: "HR_Rooster.ics");
        }
    }
}
