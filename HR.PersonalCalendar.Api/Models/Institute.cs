using HR.WebUntisConnector.Configuration;

using System.Text.Json.Serialization;

namespace HR.PersonalCalendar.Api.Models
{
    public class Institute
    {
        public static Institute FromInstituteElement(InstituteElement institute) => new()
        {
            Code = institute.Code,
            Name = institute.Name,
            DisplayName = string.IsNullOrEmpty(institute.DisplayName) ? institute.Name : institute.DisplayName,
            IsVisible = institute.IsVisible
        };

        public string Code { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        [JsonPropertyName("visible")]
        public bool IsVisible { get; set; }
    }
}
