﻿using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Extensions;
using HR.PersonalTimetable.Api.Models;
using HR.PersonalTimetable.Api.Queries;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimetablesController : ControllerBase
    {
        private readonly IQueryDispatcher queryDispatcher;
        private readonly AppSettings appSettings;

        public TimetablesController(IQueryDispatcher queryDispatcher, IOptions<AppSettings> appSettings)
        {
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
            this.appSettings = Ensure.Argument.NotNull(() => appSettings).Value;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPersonalizedExportAsync([FromRoute, FromQuery] GetTimetableSchedules query, CancellationToken cancellationToken = default)
        {
            var calendarData = (await GetPersonalizedAsync(query, cancellationToken)).SelectMany(schedule => schedule.TimetableGroups).ExportCalendar(appSettings.ExportRefreshIntervalInMinutes);
            return File(Encoding.UTF8.GetBytes(calendarData), contentType: "text/calendar", fileDownloadName: "HR_Rooster.ics");
        }
    }
}