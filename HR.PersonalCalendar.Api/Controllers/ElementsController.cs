using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

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
    public class ElementsController : ControllerBase
    {
        private readonly IQueryDispatcher queryDispatcher;

        public ElementsController(IQueryDispatcher queryDispatcher)
        {
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Element>>> GetAsync([FromQuery] GetElements query, CancellationToken cancellationToken = default)
        {
            return Ok(await queryDispatcher.DispatchAsync(query, cancellationToken));
        }
    }
}
