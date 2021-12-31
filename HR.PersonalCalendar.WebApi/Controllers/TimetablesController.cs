using Developist.Core.Cqrs;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Infrastructure;
using HR.PersonalCalendar.Queries;
using HR.PersonalCalendar.WebApi.Models;
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

        public TimetablesController(
            IClock clock,
            IWebHostEnvironment environment,
            IConfiguration configuration,
            IDispatcher dispatcher) : base(environment, configuration, dispatcher) => this.clock = Ensure.Argument.NotNull(() => clock);

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
        public async Task<ActionResult<IEnumerable<TimetableGroup>>> GetPersonalizedAsync([FromQuery(Name = "user"), BindRequired] string userName, CancellationToken cancellationToken = default)
        {
            var preferences = await QueryDispatcher.DispatchAsync(new GetPersonalTimetables { UserName = userName }, cancellationToken);
            foreach (var preference in preferences)
            {
                var timetableGroups = await QueryDispatcher.DispatchAsync(new GetTimetableGroups
                {
                    InstituteName = preference.InstituteName,
                    Element = PersonalizationModel.FromPersonalTimetable(preference).ToElement(),
                    StartDate = clock.Now.Date.GetFirstWeekday(),
                    EndDate = clock.Now.Date.GetLastWeekday().AddDays(1)
                }, cancellationToken);
            }

            // return timetable groups
            return null;
        }
    }
}
