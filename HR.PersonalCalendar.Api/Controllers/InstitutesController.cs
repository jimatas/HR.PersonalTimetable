using Developist.Core.Utilities;

using HR.PersonalCalendar.Api.Models;
using HR.WebUntisConnector.Configuration;

using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.Linq;

namespace HR.PersonalCalendar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstitutesController : ControllerBase
    {
        private readonly WebUntisConfigurationSection configuration;

        public InstitutesController(WebUntisConfigurationSection configuration)
        {
            this.configuration = Ensure.Argument.NotNull(() => configuration);
        }

        [HttpGet]
        public ActionResult<IEnumerable<Institute>> Get()
        {
            var institutes = configuration.Schools.SelectMany(school => school.Institutes).Where(institute => institute.IsVisible).Select(Institute.FromInstituteElement);
            return Ok(institutes);
        }
    }
}
