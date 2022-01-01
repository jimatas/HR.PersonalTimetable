using HR.PersonalCalendar.Model;
using HR.WebUntisConnector.Model;

using System;
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
        public static PersonalizationModel FromPersonalTimetable(PersonalTimetable personalTimetable) => new()
        {
            Id = personalTimetable.Id,
            UserName = personalTimetable.UserName,
            InstituteName = personalTimetable.InstituteName,
            ElementType = personalTimetable.ElementType,
            ElementId = personalTimetable.ElementId,
            ElementName = personalTimetable.ElementName,
            IsVisible = personalTimetable.IsVisible
        };

        public Guid? Id { get; set; }

        [Required]
        [StringLength(25)]
        [RegularExpression("[0-9]{7,8}|[a-zA-Z]{5}", ErrorMessage = "The field {0} must be either a student number or an employee code.")]
        public string UserName { get; set; }
        
        [Required]
        [StringLength(50)]
        public string InstituteName { get; set; }

        [Required]
        public ElementType ElementType { get; set; } = ElementType.Klasse;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "The field {0} must be an integer between {1} and {2}.")]
        public int ElementId { get; set; } = 0;

        [Required]
        [StringLength(100)]
        public string ElementName { get; set; }

        [JsonPropertyName("visible")]
        public bool IsVisible { get; set; } = true;
    }
}
