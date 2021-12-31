using Developist.Core.Utilities;

using HR.PersonalCalendar.Model;
using HR.WebUntisConnector.Model;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HR.PersonalCalendar.WebApi.Models
{
    public class PersonalizationModel
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PersonalizationModel"/> class with its properties initialized from the corresponding ones in the specified <see cref="PersonalTimetable"/> object.
        /// </summary>
        /// <param name="personalTimetable"></param>
        /// <returns></returns>
        public static PersonalizationModel FromPersonalTimetable(PersonalTimetable personalTimetable)
        {
            Ensure.Argument.NotNull(() => personalTimetable);

            return new()
            {
                UserName = Ensure.Argument.NotNullOrEmpty(personalTimetable.UserName, $"{nameof(personalTimetable)}.{nameof(personalTimetable.UserName)}"),
                InstituteName = Ensure.Argument.NotNullOrEmpty(personalTimetable.InstituteName, $"{nameof(personalTimetable)}.{nameof(personalTimetable.InstituteName)}"),
                ElementType = Ensure.Argument.NotOutOfRange(personalTimetable.ElementType, $"{nameof(personalTimetable)}.{nameof(personalTimetable.ElementType)}"),
                ElementId = personalTimetable.ElementId,
                ElementName = Ensure.Argument.NotNullOrEmpty(personalTimetable.ElementName, $"{nameof(personalTimetable)}.{nameof(personalTimetable.ElementName)}"),
                IsVisible = personalTimetable.IsVisible
            };
        }

        [Required]
        public string UserName { get; set; }
        [Required]
        public string InstituteName { get; set; }
        [Required]
        public ElementType ElementType { get; set; }
        public int? ElementId { get; set; }
        [Required]
        public string ElementName { get; set; }
        [JsonPropertyName("visible")]
        public bool IsVisible { get; set; }
    }
}
