using Developist.Core.Utilities;

using HR.WebUntisConnector.Configuration;

namespace HR.PersonalCalendar.WebApi.Models
{
    public class InstituteModel
    {
        public static InstituteModel FromElement(InstituteElement element)
        {
            Ensure.Argument.NotNull(() => element);

            return new InstituteModel
            {
                Code = Ensure.Argument.NotNullOrEmpty(element.Code, $"{nameof(element)}.{nameof(Code)}"),
                Name = Ensure.Argument.NotNullOrEmpty(element.Name, $"{nameof(element)}.{nameof(Name)}"),
                DisplayName = string.IsNullOrEmpty(element.DisplayName) ? element.Name : element.DisplayName
            };
        }

        public string Code { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }
}
