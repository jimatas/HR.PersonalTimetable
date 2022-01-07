using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Models;
using HR.WebUntisConnector.Configuration;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;

namespace HR.PersonalTimetable.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public class InstitutesController : ControllerBase
    {
        private readonly WebUntisConfigurationSection configuration;

        public InstitutesController(WebUntisConfigurationSection configuration)
        {
            this.configuration = Ensure.Argument.NotNull(() => configuration);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IEnumerable<Institute> Get()
        {
            var institutes = configuration.Schools.SelectMany(school => school.Institutes).Where(institute => institute.IsVisible).Select(Institute.FromInstituteElement);
            return institutes;
        }
    }
}
