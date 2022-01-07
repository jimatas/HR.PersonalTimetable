using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Queries;
using HR.WebUntisConnector.Model;

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
    [Produces(MediaTypeNames.Application.Json)]
    public class ElementsController : ControllerBase
    {
        private readonly IQueryDispatcher queryDispatcher;

        public ElementsController(IQueryDispatcher queryDispatcher)
        {
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IEnumerable<Element>> GetAsync([FromQuery] GetElements query, CancellationToken cancellationToken = default)
        {
            return await queryDispatcher.DispatchAsync(query, cancellationToken);
        }
    }
}
