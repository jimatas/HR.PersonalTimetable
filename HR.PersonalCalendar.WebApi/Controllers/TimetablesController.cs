using Developist.Core.Cqrs;

using HR.PersonalCalendar.Queries;
using HR.WebUntisConnector.Model;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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
        public TimetablesController(
            IWebHostEnvironment environment,
            IConfiguration configuration,
            IDispatcher dispatcher) : base(environment, configuration, dispatcher)
        {
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TimetableGroup>>> GetAsync(
            [FromQuery(Name = "institute")] string instituteName,
            [FromQuery(Name = "element")] ElementType elementType,
            [FromQuery(Name = "name")] string elementName,
            [FromQuery(Name = "start")] DateTime startDate,
            [FromQuery(Name = "end")] DateTime endDate,
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
                StartDate = startDate,
                EndDate = endDate
            }, cancellationToken);

            return Ok(timetableGroups);
        }

        [HttpGet("personalized")]
        public Task<ActionResult<IEnumerable<TimetableGroup>>> GetPersonalizedAsync([FromQuery(Name = "user")] string userName, CancellationToken cancellationToken = default)
        {
            return null;
        }
    }
}
