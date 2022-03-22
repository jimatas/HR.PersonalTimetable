using HR.Common.Persistence.Entities;
using HR.WebUntisConnector.Model;

using System;
using System.Text.Json.Serialization;

namespace HR.PersonalTimetable.Application.Models
{
    /// <summary>
    /// Represents the personal timetable of a user.
    /// </summary>
    public class PersonalTimetable : EntityBase<Guid>
    {
        public PersonalTimetable() { }
        public PersonalTimetable(Guid id) => Id = id;

        [JsonIgnore]
        public override bool IsTransient => base.IsTransient;

        /// <summary>
        /// The integration through which this personal timetable was created.
        /// </summary>
        [JsonIgnore]
        public Integration Integration { get; set; }

        /// <summary>
        /// Depending on whether the user is a student or an employee, either their student number or their employee code.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The RUAS institute that the timetable element falls under.
        /// </summary>
        public string InstituteName { get; set; }

        /// <summary>
        /// The type of the element.
        /// </summary>
        /// <seealso cref="Element.Type"/>
        public ElementType ElementType { get; set; }

        /// <summary>
        /// The primary key of the element in the WebUntis database.
        /// </summary>
        /// <seealso cref="Element.Id"/>
        public int ElementId { get; set; }

        /// <summary>
        /// The abbreviated name of the element. 
        /// Provides an alternative way of identifiying elements.
        /// </summary>
        /// <seealso cref="Element.Name"/>
        public string ElementName { get; set; }

        /// <summary>
        /// If <see cref="ElementType"/> is <see cref="ElementType.Klasse"/>, the ID of the school year that it is defined in.
        /// </summary>
        public int? SchoolYearId { get; set; }

        /// <summary>
        /// Allows the user to temporarily hide a timetable without having to delete it from their personal timetable.
        /// Default value is <c>true</c>.
        /// </summary>
        [JsonPropertyName("visible")]
        public bool IsVisible { get; set; } = true;

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateLastModified { get; set; }
    }
}
