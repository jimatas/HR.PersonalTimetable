using Developist.Core.Cqrs;

using HR.PersonalCalendar.Commands;
using HR.PersonalCalendar.Queries;
using HR.PersonalCalendar.WebApi.Filters;
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
        public async Task<IEnumerable<Personalization>> GetAsync([FromQuery(Name = "user"), BindRequired] string userName, CancellationToken cancellationToken = default)
        {
            var personalTimetables = await QueryDispatcher.DispatchAsync(new GetPersonalTimetables { UserName = userName }, cancellationToken);
            return personalTimetables.Select(Personalization.FromPersonalTimetable);
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync([FromBody] PersonalizationParameters personalization, CancellationToken cancellationToken = default)
        {
            if (!User.Identity.Name.Equals(personalization.UserName, StringComparison.InvariantCultureIgnoreCase))
            {
                return Unauthorized();
            }

            if (personalization.ElementId.GetValueOrDefault() == default && string.IsNullOrEmpty(personalization.ElementName))
            {
                ModelState.AddModelError(nameof(personalization.ElementId), "Either the element's id or its name must be specified.");
                ModelState.AddModelError(nameof(personalization.ElementName), "Either the element's id or its name must be specified.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            await CommandDispatcher.DispatchAsync(new AddPersonalTimetable
            {
                UserName = personalization.UserName,
                InstituteName = personalization.InstituteName,
                ElementType = personalization.ElementType,
                ElementId = personalization.ElementId,
                ElementName = personalization.ElementName
            }, cancellationToken);

            return CreatedAtRoute("GetForUser", new { user = personalization.UserName }, personalization);
        }

        [HttpPatch]
        [UnauthorizedExceptionFilter]
        public async Task<ActionResult> PatchAsync(
            [FromQuery(Name = "id"), BindRequired] Guid personalTimetableId,
            [FromQuery(Name = "visible"), BindRequired] bool isVisible,
            CancellationToken cancellationToken = default)
        {
            await CommandDispatcher.DispatchAsync(new ChangeTimetableVisibility
            {
                PersonalTimetableId = personalTimetableId,
                IsVisible = isVisible,
                UserNameToVerify = User.Identity.Name
            }, cancellationToken);

            return NoContent();
        }

        [HttpDelete]
        [UnauthorizedExceptionFilter]
        public async Task<ActionResult> DeleteAsync([FromQuery(Name = "id"), BindRequired] Guid personalTimetableId, CancellationToken cancellationToken = default)
        {
            await CommandDispatcher.DispatchAsync(new RemovePersonalTimetable
            {
                PersonalTimetableId = personalTimetableId,
                UserNameToVerify = User.Identity.Name
            }, cancellationToken);

            return NoContent();
        }
    }
}
