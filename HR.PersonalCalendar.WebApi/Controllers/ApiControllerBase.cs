using Developist.Core.Cqrs;
using Developist.Core.Cqrs.Commands;
using Developist.Core.Cqrs.Events;
using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace HR.PersonalCalendar.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiControllerBase : ControllerBase
    {
        private readonly IDispatcher dispatcher;

        protected ApiControllerBase(
            IWebHostEnvironment environment,
            IConfiguration configuration,
            IDispatcher dispatcher)
        {
            Environment = Ensure.Argument.NotNull(() => environment);
            Configuration = Ensure.Argument.NotNull(() => configuration);
            this.dispatcher = Ensure.Argument.NotNull(() => dispatcher);
        }

        protected IWebHostEnvironment Environment { get; }
        protected IConfiguration Configuration { get; }

        // IDispatcher derived properties
        protected ICommandDispatcher CommandDispatcher => dispatcher;
        protected IQueryDispatcher QueryDispatcher => dispatcher;
        protected IEventDispatcher EventDispatcher => dispatcher;
    }
}
