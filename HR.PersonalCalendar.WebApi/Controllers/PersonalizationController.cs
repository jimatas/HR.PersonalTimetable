﻿using Developist.Core.Cqrs;

using HR.PersonalCalendar.Commands;
using HR.PersonalCalendar.Queries;
using HR.PersonalCalendar.WebApi.Models;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.WebApi.Controllers
{
    public class PersonalizationController : ApiControllerBase
    {
        public PersonalizationController(
            IWebHostEnvironment environment,
            IConfiguration configuration,
            IDispatcher dispatcher) : base(environment, configuration, dispatcher)
        {
        }

        [HttpGet(Name = "GetForUser")]
        public async Task<IEnumerable<PersonalizationModel>> GetAsync([FromQuery(Name = "user"), BindRequired] string userName, CancellationToken cancellationToken = default)
        {
            var timetables = await QueryDispatcher.DispatchAsync(new GetPersonalTimetables { UserName = userName }, cancellationToken);
            return timetables.Select(PersonalizationModel.FromEntity);
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync([FromBody] PersonalizationModel personalization, CancellationToken cancellationToken = default)
        {
            await CommandDispatcher.DispatchAsync(new CreatePersonalTimetable
            {
                UserName = personalization.UserName,
                InstituteName = personalization.InstituteName,
                ElementType = personalization.ElementType,
                ElementName = personalization.ElementName,
                SchoolYearId = personalization.SchoolYearId
            }, cancellationToken);

            return CreatedAtRoute("GetForUser", new { user = personalization.UserName }, personalization);
        }
    }
}
