using Developist.Core.Utilities;

using HR.WebUntisConnector.Configuration;

namespace HR.PersonalCalendar.WebApi.Models
{
    public class InstituteModel
    {
        /// <summary>
        /// Creates a new instance of the <see cref="InstituteModel"/> class with its properties initialized from the corresponding ones in the specified <see cref="InstituteElement"/> object.
        /// </summary>
        /// <param name="institute"></param>
        /// <returns></returns>
        public static InstituteModel FromElement(InstituteElement institute)
        {
            Ensure.Argument.NotNull(() => institute);

            return new()
            {
                Code = institute.Code,
                Name = Ensure.Argument.NotNullOrEmpty(institute.Name, $"{nameof(institute)}.{nameof(Name)}"),
                DisplayName = string.IsNullOrEmpty(institute.DisplayName) ? institute.Name : institute.DisplayName
            };
        }

        public string Code { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }
}
