using HR.WebUntisConnector.Model;

using System;
using System.ComponentModel.DataAnnotations;

namespace HR.PersonalCalendar.WebApi.Models
{
    public class PersonalizationWriteModel
    {
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
    }
}
