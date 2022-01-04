using Developist.Core.Cqrs.Commands;
using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Api.Commands;
using HR.PersonalCalendar.Api.Filters;
using HR.PersonalCalendar.Api.Models;
using HR.PersonalCalendar.Api.Queries;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
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
        public async Task<ActionResult<IEnumerable<PersonalTimetable>>> GetAsync([FromRoute] GetPersonalTimetables query, CancellationToken cancellationToken = default)
        {
            return Ok(await queryDispatcher.DispatchAsync(query, cancellationToken));
        }

        [HttpPost]
        [ApiExceptionFilter]
        public async Task<ActionResult<PersonalTimetable>> PostAsync([FromBody] AddPersonalTimetable command, CancellationToken cancellationToken = default)
        {
            await commandDispatcher.DispatchAsync(command, cancellationToken);
            var lastTimetableAdded = (await queryDispatcher.DispatchAsync(new GetPersonalTimetables { UserName = command.UserName }, cancellationToken)).OrderBy(timetable => timetable.DateCreated).Last();
            return CreatedAtRoute("GetForUser", new { user = lastTimetableAdded.UserName }, lastTimetableAdded);
        }

        [HttpPatch]
        [ApiExceptionFilter]
        public async Task<IActionResult> PatchAsync([FromBody] ChangeTimetableVisibility command, CancellationToken cancellationToken = default)
        {
            await commandDispatcher.DispatchAsync(command, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ApiExceptionFilter]
        public async Task<IActionResult> DeleteAsync([FromRoute] RemovePersonalTimetable command, CancellationToken cancellationToken = default)
        {
            await commandDispatcher.DispatchAsync(command, cancellationToken);
            return NoContent();
        }
    }
}
