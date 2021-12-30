using Developist.Core.Cqrs;

using HR.PersonalCalendar.Queries;
using HR.PersonalCalendar.WebApi.Models;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.WebApi.Controllers
{
    public class PersonalizationController : ApiControllerBase
    {
        public PersonalizationController(
            IWebHostEnvironment environment,
            IConfiguration configuration,
            IDispatcher dispatcher) : base(environment, configuration, dispatcher)
        {
        }

        [HttpGet]
        public async Task<IEnumerable<PersonalizationModel>> GetAsync([FromQuery(Name = "user"), BindRequired] string userName, CancellationToken cancellationToken = default)
        {
            var timetables = await QueryDispatcher.DispatchAsync(new GetPersonalTimetables { UserName = userName }, cancellationToken);
            return timetables.Select(PersonalizationModel.FromEntity);
        }
    }
}
