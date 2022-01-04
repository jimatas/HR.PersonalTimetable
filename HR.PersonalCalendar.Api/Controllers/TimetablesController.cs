using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Api.Extensions;
using HR.PersonalCalendar.Api.Filters;
using HR.PersonalCalendar.Api.Queries;
using HR.WebUntisConnector.Model;

using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public async Task<IEnumerable<TimetableGroup>> GetAsync([FromQuery] GetTimetableGroups query, CancellationToken cancellationToken = default)
        {
            return await queryDispatcher.DispatchAsync(query, cancellationToken);
        }

        [HttpGet("personalized/{user}")]
        public async Task<IEnumerable<Models.PersonalCalendar>> GetPersonalizedAsync([FromRoute, FromQuery] GetPersonalCalendars query, CancellationToken cancellationToken = default)
        {
            return await queryDispatcher.DispatchAsync(query, cancellationToken);
        }

        [HttpGet("personalized/{user}/export")]
        public async Task<IActionResult> GetPersonalizedExportAsync([FromRoute, FromQuery] GetPersonalCalendars query, CancellationToken cancellationToken = default)
        {
            var calendarData = (await GetPersonalizedAsync(query, cancellationToken)).SelectMany(calendar => calendar.TimetableGroups).ExportCalendar();
            return File(Encoding.UTF8.GetBytes(calendarData), contentType: "text/calendar", fileDownloadName: "HR_Rooster.ics");
        }
    }
}
