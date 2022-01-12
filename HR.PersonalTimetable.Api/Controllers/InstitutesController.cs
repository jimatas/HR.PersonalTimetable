using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Models;
using HR.PersonalTimetable.Api.Queries;
using HR.WebUntisConnector.Configuration;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
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
    public class InstitutesController : ControllerBase
    {
        private readonly IQueryDispatcher queryDispatcher;
        private readonly WebUntisConfigurationSection configuration;

        public InstitutesController(IQueryDispatcher queryDispatcher, WebUntisConfigurationSection configuration)
        {
            this.configuration = Ensure.Argument.NotNull(() => configuration);
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IEnumerable<Institute> Get()
        {
            var institutes = configuration.Schools.SelectMany(school => school.Institutes).Where(institute => institute.IsVisible).Select(Institute.FromInstituteElement);
            return institutes;
        }

        [HttpGet("{institute}/last-imported")]
        [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<DateTime> GetLastImportAsync([FromRoute] GetLastImportTime query, CancellationToken cancellationToken = default)
        {
            return await queryDispatcher.DispatchAsync(query, cancellationToken);
        }
    }
}
