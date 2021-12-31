﻿using Developist.Core.Cqrs;
using Developist.Core.Utilities;

using HR.PersonalCalendar.WebApi.Models;
using HR.WebUntisConnector.Configuration;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using System.Collections.Generic;
using System.Linq;

namespace HR.PersonalCalendar.WebApi.Controllers
{
    public class InstitutesController : ApiControllerBase
    {
        private readonly WebUntisConfigurationSection configuration;

        public InstitutesController(
            WebUntisConfigurationSection webuntisConfiguration,
            IWebHostEnvironment environment,
            IConfiguration configuration,
            IDispatcher dispatcher) : base(environment, configuration, dispatcher) => this.configuration = Ensure.Argument.NotNull(() => webuntisConfiguration);

        [HttpGet]
        public IEnumerable<InstituteModel> Get()
        {
            var instituteModels = configuration.Schools.SelectMany(school => school.Institutes)
                .Where(institute => institute.IsVisible)
                .Select(InstituteModel.FromInstituteElement);

            return instituteModels;
        }
    }
}
