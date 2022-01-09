using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Models;
using HR.PersonalTimetable.Api.Queries;

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

        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<TimetableSchedule> GetAsync([FromQuery] GetTimetableSchedule query, CancellationToken cancellationToken = default)
        {
            return await queryDispatcher.DispatchAsync(query, cancellationToken);
        }

        [HttpGet("personalized/{user}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IEnumerable<TimetableSchedule>> GetPersonalizedAsync([FromRoute, FromQuery] GetTimetableSchedules query, CancellationToken cancellationToken = default)
        {
            return await queryDispatcher.DispatchAsync(query, cancellationToken);
        }

        [HttpGet("personalized/{user}/export")]
        [Produces("text/calendar")]
        [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<FileContentResult> GetPersonalizedExportAsync([FromRoute] GetTimetableSchedulesForExport query, CancellationToken cancellationToken = default)
        {
            return await queryDispatcher.DispatchAsync(query, cancellationToken);
        }
    }
}
