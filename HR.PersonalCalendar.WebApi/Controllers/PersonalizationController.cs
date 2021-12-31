using Developist.Core.Cqrs;

using HR.PersonalCalendar.Commands;
using HR.PersonalCalendar.Queries;
using HR.PersonalCalendar.WebApi.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.WebApi.Controllers
{
    [Authorize]
    public class PersonalizationController : ApiControllerBase
    {
        public PersonalizationController(
            IWebHostEnvironment environment,
            IConfiguration configuration,
            IDispatcher dispatcher) : base(environment, configuration, dispatcher)
        {
        }

        [HttpGet(Name = "GetForUser")]
        public async Task<IEnumerable<PersonalizationModel>> GetAsync([FromQuery(Name = "user"), BindRequired] string userName, CancellationToken cancellationToken = default)
        {
            var personalTimetables = await QueryDispatcher.DispatchAsync(new GetPersonalTimetables { UserName = userName }, cancellationToken);
            return personalTimetables.Select(PersonalizationModel.FromPersonalTimetable);
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync([FromBody] PersonalizationModel personalizationModel, CancellationToken cancellationToken = default)
        {
            if (!User.Identity.Name.Equals(personalizationModel.UserName, StringComparison.InvariantCultureIgnoreCase))
            {
                return Unauthorized();
            }

            await CommandDispatcher.DispatchAsync(new CreatePersonalTimetable
            {
                UserName = personalizationModel.UserName,
                InstituteName = personalizationModel.InstituteName,
                ElementType = personalizationModel.ElementType,
                ElementName = personalizationModel.ElementName
            }, cancellationToken);

            return CreatedAtRoute("GetForUser", new { user = personalizationModel.UserName }, personalizationModel);
        }

        [HttpPatch]
        public async Task<ActionResult<PersonalizationModel>> PatchAsync([FromBody] PersonalizationModel personalizationModel, CancellationToken cancellationToken = default)
        {
            if (!User.Identity.Name.Equals(personalizationModel.UserName, StringComparison.InvariantCultureIgnoreCase))
            {
                return Unauthorized();
            }

            await CommandDispatcher.DispatchAsync(new ChangeTimetableVisibility
            {
                UserName = personalizationModel.UserName,
                InstituteName = personalizationModel.InstituteName,
                ElementType = personalizationModel.ElementType,
                ElementName = personalizationModel.ElementName,
                IsVisible = personalizationModel.IsVisible
            }, cancellationToken);

            return Ok(personalizationModel);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteAsync([FromBody] PersonalizationModel personalizationModel, CancellationToken cancellationToken = default)
        {
            if (!User.Identity.Name.Equals(personalizationModel.UserName, StringComparison.InvariantCultureIgnoreCase))
            {
                return Unauthorized();
            }

            await CommandDispatcher.DispatchAsync(new RemovePersonalTimetable
            {
                UserName = personalizationModel.UserName,
                InstituteName = personalizationModel.InstituteName,
                ElementType = personalizationModel.ElementType,
                ElementName = personalizationModel.ElementName
            }, cancellationToken);

            return NoContent();
        }
    }
}
