using HR.WebUntisConnector.Model;

using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace HR.PersonalTimetable.Application.Models
{
    public class LessonDetails
    {
        /// <summary>
        /// The expanded WebUntis lesson number.
        /// For instance, 79901.
        /// </summary>
        /// <example>79901</example>
        [JsonPropertyName("lsnumber")]
        public int LessonNumber { get; set; }

        /// <summary>
        /// Any remarks entered for this lesson.
        /// </summary>
        [JsonPropertyName("lstext")]
        public string LessonText { get; set; }

        /// <summary>
        /// The distinct set of teachers linked to the WebUntis timetables that make up this lesson.
        /// </summary>
        /// <remarks>
        /// This array will generally contain only one element.
        /// </remarks>
        [JsonPropertyName("te")]
        public IEnumerable<Teacher> Teachers { get; set; } = Enumerable.Empty<Teacher>();

        /// <summary>
        /// The distinct set of klassen linked to the WebUntis timetables that make up this lesson.
        /// </summary>
        [JsonPropertyName("kl")]
        public IEnumerable<Klasse> Klassen { get; set; } = Enumerable.Empty<Klasse>();

        /// <summary>
        /// The distinct set of rooms linked to the WebUntis timetables that make up this lesson.
        /// </summary>
        [JsonPropertyName("ro")]
        public IEnumerable<Room> Rooms { get; set; } = Enumerable.Empty<Room>();
    }
}
