﻿using Developist.Core.Cqrs.Commands;
using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Api.Commands;
using HR.PersonalCalendar.Api.Models;
using HR.PersonalCalendar.Api.Queries;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class PersonalizationController : ControllerBase
    {
        private readonly ICommandDispatcher commandDispatcher;
        private readonly IQueryDispatcher queryDispatcher;

        public PersonalizationController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
        {
            this.commandDispatcher = Ensure.Argument.NotNull(() => commandDispatcher);
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
        }

        [HttpGet("{user}", Name = "GetForUser")]
        [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IEnumerable<PersonalTimetable>> GetAsync([FromRoute] GetPersonalTimetables query, CancellationToken cancellationToken = default)
        {
            return await queryDispatcher.DispatchAsync(query, cancellationToken);
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PersonalTimetable>> PostAsync([FromBody] AddPersonalTimetable command, CancellationToken cancellationToken = default)
        {
            await commandDispatcher.DispatchAsync(command, cancellationToken);
            var lastTimetableAdded = (await queryDispatcher.DispatchAsync(new GetPersonalTimetables { UserName = command.UserName }, cancellationToken)).OrderBy(timetable => timetable.DateCreated).Last();
            return CreatedAtRoute("GetForUser", new { user = lastTimetableAdded.UserName }, lastTimetableAdded);
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PatchAsync([FromRoute, FromQuery] ChangeTimetableVisibility command, CancellationToken cancellationToken = default)
        {
            await commandDispatcher.DispatchAsync(command, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync([FromRoute] RemovePersonalTimetable command, CancellationToken cancellationToken = default)
        {
            await commandDispatcher.DispatchAsync(command, cancellationToken);
            return NoContent();
        }
    }
}
