using Developist.Core.Cqrs;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Infrastructure;
using HR.PersonalCalendar.Queries;
using HR.WebUntisConnector;
using HR.WebUntisConnector.Model;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.WebApi.Controllers
{
    public class TimetablesController : ApiControllerBase
    {
        private readonly IApiClientFactory apiClientFactory;

        public TimetablesController(
            ICachedApiClientFactory apiClientFactory,
            IWebHostEnvironment environment,
            IConfiguration configuration,
            IDispatcher dispatcher) : base(environment, configuration, dispatcher)
        {
            this.apiClientFactory = Ensure.Argument.NotNull(() => apiClientFactory);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TimetableGroup>>> GetAsync(
            string instituteName,
            ElementType elementType,
            string elementName,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            var element = await QueryDispatcher.DispatchAsync(new LookupElement
            {
                InstituteName = instituteName,
                ElementName = elementName,
                ElementType = elementType
            }, cancellationToken);

            if (element is null)
            {
                return NotFound();
            }

            var timetableGroups = await QueryDispatcher.DispatchAsync(new GetTimetableGroups
            {
                InstituteName = instituteName,
                Element = element,
                StartDate = startDate,
                EndDate = endDate
            }, cancellationToken);

            return Ok(timetableGroups);
        }
    }
}
