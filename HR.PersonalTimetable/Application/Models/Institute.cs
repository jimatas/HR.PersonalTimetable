using HR.WebUntisConnector.Configuration;

using System.Text.Json.Serialization;

namespace HR.PersonalTimetable.Application.Models
{
    public class Institute
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Institute"/> class with its properties initialized from the corresponding ones in the specified <see cref="InstituteElement"/> object.
        /// </summary>
        /// <param name="institute"></param>
        /// <returns></returns>
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
