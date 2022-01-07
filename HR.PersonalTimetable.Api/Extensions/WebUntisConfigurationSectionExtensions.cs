using HR.WebUntisConnector.Configuration;

using System;
using System.Linq;

namespace HR.PersonalTimetable.Api.Extensions
{
    public static class WebUntisConfigurationSectionExtensions
    {
        /// <summary>
        /// Returns the first <see cref="SchoolElement"/> whose name, or the name of one of its <see cref="InstituteElement"/> children, matches the specified value.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="schoolOrInstituteName"></param>
        /// <returns>Either the matching <see cref="SchoolElement"/> or <c>null</c>, if nothing matched.</returns>
        public static SchoolElement FindSchool(this WebUntisConfigurationSection configuration, string schoolOrInstituteName)
        {
            return configuration.Schools.FirstOrDefault(school => school.Name.Equals(schoolOrInstituteName, StringComparison.OrdinalIgnoreCase) || school.Institutes.Any(insitute => insitute.Name.Equals(schoolOrInstituteName, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
