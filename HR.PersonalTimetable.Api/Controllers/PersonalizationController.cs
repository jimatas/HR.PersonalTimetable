using Developist.Core.Cqrs.Commands;
using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Commands;
using HR.PersonalTimetable.Api.Queries;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public class PersonalizationController : ControllerBase
    {
        private readonly ICommandDispatcher commandDispatcher;
        private readonly IQueryDispatcher queryDispatcher;

        public PersonalizationController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
        {
            this.commandDispatcher = Ensure.Argument.NotNull(() => commandDispatcher);
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
        }

        /// <summary>
        /// Retrieve the personal schedule information of a user.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{user}", Name = "GetForUser")]
        [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IEnumerable<Models.PersonalTimetable>> GetAsync([FromRoute] GetPersonalTimetables query, CancellationToken cancellationToken = default)
        {
            return await queryDispatcher.DispatchAsync(query, cancellationToken);
        }

        /// <summary>
        /// Add a new timetable to the user's personal schedule.
        /// </summary>
        /// <remarks>
        /// Note that either the element's id or its name (or both) must be provided.
        /// If only one of them is supplied, a lookup will be done to retrieve the value of the other. 
        /// If both are supplied, their values will be saved as-is.
        /// </remarks>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Models.PersonalTimetable>> PostAsync([FromBody] AddPersonalTimetable command, CancellationToken cancellationToken = default)
        {
            await commandDispatcher.DispatchAsync(command, cancellationToken);
            var lastTimetableAdded = (await queryDispatcher.DispatchAsync(new GetPersonalTimetables { UserName = command.UserName }, cancellationToken)).OrderBy(timetable => timetable.DateCreated).Last();
            return CreatedAtRoute("GetForUser", new { user = lastTimetableAdded.UserName }, lastTimetableAdded);
        }

        /// <summary>
        /// Allow a user to temporarily hide a timetable from their personal schedule without having to delete it.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PatchAsync([FromRoute, FromQuery] ChangeTimetableVisibility command, CancellationToken cancellationToken = default)
        {
            await commandDispatcher.DispatchAsync(command, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Permanently remove a timetable from the personal schedule of a user.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteAsync([FromRoute] RemovePersonalTimetable command, CancellationToken cancellationToken = default)
        {
            await commandDispatcher.DispatchAsync(command, cancellationToken);
            return NoContent();
        }
    }
}
