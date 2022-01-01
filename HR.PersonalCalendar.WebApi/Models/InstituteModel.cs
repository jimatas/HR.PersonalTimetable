using HR.WebUntisConnector.Configuration;

namespace HR.PersonalCalendar.WebApi.Models
{
    public class InstituteModel
    {
        /// <summary>
        /// Creates a new instance of the <see cref="InstituteModel"/> class with its properties initialized from the corresponding ones in the specified <see cref="InstituteElement"/> object.
        /// </summary>
        /// <param name="instituteElement"></param>
        /// <returns></returns>
        public static InstituteModel FromInstituteElement(InstituteElement instituteElement) => new()
        {
            Code = instituteElement.Code,
            Name = instituteElement.Name,
            DisplayName = string.IsNullOrEmpty(instituteElement.DisplayName) ? instituteElement.Name : instituteElement.DisplayName
        };

        public string Code { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }
}
