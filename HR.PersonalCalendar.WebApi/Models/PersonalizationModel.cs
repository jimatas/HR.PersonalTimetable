using Developist.Core.Utilities;

using HR.PersonalCalendar.Model;
using HR.WebUntisConnector.Model;

using System.Text.Json.Serialization;

namespace HR.PersonalCalendar.WebApi.Models
{
    public class PersonalizationModel
    {
        public PersonalizationModel() { }

        /// <summary>
        /// Mapping constructor. 
        /// Maps the properties of the specified <see cref="PersonalTimetable"/> entity to the corresponding properties in this <see cref="PersonalizationModel"/> object.
        /// </summary>
        /// <param name="table"></param>
        public PersonalizationModel(PersonalTimetable table)
        {
            Ensure.Argument.NotNull(() => table);

            UserName = table.UserName;
            InstituteName = table.InstituteName;
            ElementType = table.ElementType;
            ElementId = table.ElementId;
            ElementName = table.ElementName;
            SchoolYearId = table.SchoolYearId;
            IsVisible = table.IsVisible;
        }

        public string UserName { get; set; }
        public string InstituteName { get; set; }
        public ElementType ElementType { get; set; }
        public int? ElementId { get; set; }
        public string ElementName { get; set; }
        public int? SchoolYearId { get; set; }
        [JsonPropertyName("visible")]
        public bool IsVisible { get; set; }
    }
}
