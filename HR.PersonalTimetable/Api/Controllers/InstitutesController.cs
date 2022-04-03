using HR.Common.Cqrs.Queries;
using HR.Common.Utilities;
using HR.PersonalTimetable.Application.Models;
using HR.PersonalTimetable.Application.Queries;
using HR.WebUntisConnector.Model;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
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

        public InstitutesController(IQueryDispatcher queryDispatcher)
        {
            this.queryDispatcher = Ensure.Argument.NotNull(() => queryDispatcher);
        }

        /// <summary>
        /// Retrieve a listing with all the RUAS institutes.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(Name = nameof(GetInstitutes))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IEnumerable<Institute>> GetAsync([FromQuery] GetInstitutes query, CancellationToken cancellationToken = default)
        {
            return await queryDispatcher.DispatchAsync(query, cancellationToken);
        }

        /// <summary>
        /// Get the timestamp of when timetables were last imported from Untis.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{institute}/last-imported", Name = nameof(GetLastImportTime))]
        [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<DateTime> GetLastImportedAsync([FromRoute] GetLastImportTime query, CancellationToken cancellationToken = default)
        {
            return await queryDispatcher.DispatchAsync(query, cancellationToken);
        }

        /// <summary>
        /// Retrieve all the holidays defined for a particular RUAS institute.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{institute}/holidays", Name = nameof(GetHolidays))]
        [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IEnumerable<Holiday>> GetHolidaysAsync([FromRoute, FromQuery] GetHolidays query, CancellationToken cancellationToken = default)
        {
            return await queryDispatcher.DispatchAsync(query, cancellationToken);
        }
    }
}
