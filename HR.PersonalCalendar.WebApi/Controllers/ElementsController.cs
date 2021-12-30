using Developist.Core.Cqrs;

using HR.PersonalCalendar.Queries;
using HR.WebUntisConnector.Model;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.WebApi.Controllers
{
    public class ElementsController : ApiControllerBase
    {
        public ElementsController(
            IWebHostEnvironment environment, 
            IConfiguration configuration, 
            IDispatcher dispatcher) : base(environment, configuration, dispatcher)
        {
        }

        [HttpGet("type/{elementType}")]
        public async Task<IEnumerable<Element>> GetAsync(ElementType elementType, [FromQuery(Name = "institute"), BindRequired] string instituteName, CancellationToken cancellationToken = default)
        {
            var elements = await QueryDispatcher.DispatchAsync(new GetElementsByType { InstituteName = instituteName, ElementType = elementType }, cancellationToken);
            return elements;
        }
    }
}
