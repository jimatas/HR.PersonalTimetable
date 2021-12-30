﻿using Developist.Core.Cqrs;

using HR.PersonalCalendar.Queries;
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
        public TimetablesController(
            IWebHostEnvironment environment,
            IConfiguration configuration,
            IDispatcher dispatcher) : base(environment, configuration, dispatcher)
        {
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
                StartDate = startDate ?? DateTime.Today.GetFirstWeekday(),
                EndDate = endDate ?? DateTime.Today.GetLastWeekday().AddDays(1)
            }, cancellationToken);

            return Ok(timetableGroups);
        }

        [HttpGet("personalized")]
        public async Task<ActionResult<IEnumerable<TimetableGroup>>> GetPersonalizedAsync([FromQuery(Name = "user")] string userName, CancellationToken cancellationToken = default)
        {
            var preferences = await QueryDispatcher.DispatchAsync(new GetPersonalTimetables { UserName = userName }, cancellationToken);
            foreach (var preference in preferences)
            {
                // Get timetable group for preference
            }

            // return timetable groups
            return null;
        }
    }
}
