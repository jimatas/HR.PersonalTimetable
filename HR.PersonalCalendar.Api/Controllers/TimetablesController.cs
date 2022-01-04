using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Api.Filters;
using HR.PersonalCalendar.Api.Queries;
using HR.WebUntisConnector.Model;

using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimetablesController : ControllerBase
    {
        private readonly IQueryDispatcher queryDispatcher;

        public TimetablesController(IQueryDispatcher queryDispatcher)
        {
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
        }

        [HttpGet]
        [ApiExceptionFilter]
        public async Task<ActionResult<IEnumerable<TimetableGroup>>> GetAsync([FromQuery] GetTimetableGroups query, CancellationToken cancellationToken = default)
        {
            return Ok(await queryDispatcher.DispatchAsync(query, cancellationToken));
        }
    }
}
