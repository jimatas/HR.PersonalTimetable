using Developist.Core.Utilities;

using HR.PersonalCalendar.Model;
using HR.WebUntisConnector.Model;

using System.Text.Json.Serialization;

namespace HR.PersonalCalendar.WebApi.Models
{
    public class PersonalizationModel
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PersonalizationModel"/> class with its properties initialized from the corresponding ones in the specified <see cref="PersonalTimetable"/> object.
        /// </summary>
        /// <param name="timetable"></param>
        /// <returns></returns>
        public static PersonalizationModel FromEntity(PersonalTimetable timetable)
        {
            Ensure.Argument.NotNull(() => timetable);

            return new()
            {
                UserName = Ensure.Argument.NotNullOrEmpty(timetable.UserName, $"{nameof(timetable)}.{nameof(UserName)}"),
                InstituteName = Ensure.Argument.NotNullOrEmpty(timetable.InstituteName, $"{nameof(timetable)}.{nameof(InstituteName)}"),
                ElementType = Ensure.Argument.NotOutOfRange(timetable.ElementType, $"{nameof(timetable)}.{nameof(ElementType)}"),
                ElementId = timetable.ElementId,
                ElementName = Ensure.Argument.NotNullOrEmpty(timetable.ElementName, $"{nameof(timetable)}.{nameof(ElementName)}"),
                SchoolYearId = timetable.SchoolYearId,
                IsVisible = timetable.IsVisible
            };
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
