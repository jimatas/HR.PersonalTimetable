using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Application.Models;
using HR.PersonalTimetable.Application.Queries;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimetablesController : ControllerBase
    {
        private readonly IQueryDispatcher queryDispatcher;

        public TimetablesController(IQueryDispatcher queryDispatcher)
        {
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
        }

        /// <summary>
        /// Retrieve the timetables for a particular institute and element, which is specified by its type and id and/or name, that optionally also fall between two dates.
        /// </summary>
        /// <remarks>
        /// Note that either the element's id or its name (or both) must be provided.
        /// </remarks>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Schedule> GetAsync([FromQuery] GetSchedule query, CancellationToken cancellationToken = default)
        {
            return await queryDispatcher.DispatchAsync(query, cancellationToken);
        }

        /// <summary>
        /// Retrieve all the timetables in a  user's personal schedule that, optionally, fall between two dates.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("personalized/{user}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IEnumerable<Schedule>> GetPersonalizedAsync([FromRoute, FromQuery] GetSchedules query, CancellationToken cancellationToken = default)
        {
            return await queryDispatcher.DispatchAsync(query, cancellationToken);
        }

        /// <summary>
        /// Retrieve and export the timetables in the personal schedule of a user to iCalendar (*.ics) format.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("personalized/{user}/export")]
        [Produces("text/calendar")]
        [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<FileContentResult> GetPersonalizedExportAsync([FromRoute] GetSchedulesForExport query, CancellationToken cancellationToken = default)
        {
            return await queryDispatcher.DispatchAsync(query, cancellationToken);
        }
    }
}
