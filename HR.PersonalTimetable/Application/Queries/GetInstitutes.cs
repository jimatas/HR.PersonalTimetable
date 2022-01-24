using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Application.Models;
using HR.WebUntisConnector.Configuration;

using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Application.Queries
{
    public class GetInstitutes : IQuery<IEnumerable<Institute>>
    {
        /// <summary>
        /// Indicates whether to only return the institutes that are configured as visible in the configuration file.
        /// </summary>
        [FromQuery(Name = "visibleOnly")]
        public bool VisibleOnly { get; set; } = true;
    }

    public class GetInstitutesHandler : IQueryHandler<GetInstitutes, IEnumerable<Institute>>
    {
        private readonly WebUntisConfigurationSection configuration;

        public GetInstitutesHandler(WebUntisConfigurationSection configuration)
        {
            this.configuration = Ensure.Argument.NotNull(() => configuration);
        }

        public Task<IEnumerable<Institute>> HandleAsync(GetInstitutes query, CancellationToken cancellationToken)
        {
            var institutes = configuration.Schools.SelectMany(school => school.Institutes)
                .Where(institute => !query.VisibleOnly || institute.IsVisible)
                .Select(Institute.FromInstituteElement);

            return Task.FromResult(institutes);
        }
    }
}
