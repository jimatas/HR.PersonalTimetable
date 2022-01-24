using HR.WebUntisConnector.Model;

using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace HR.PersonalTimetable.Application.Models
{
    public class Lesson
    {
        /// <summary>
        /// The base Untis lesson number.
        /// For instance, 799.
        /// </summary>
        /// <example>799</example>
        [JsonPropertyName("lsnumber")]
        public int? LessonNumber { get; set; }

        /// <summary>
        /// The date of this lesson as an integer, formatted in ISO notation.
        /// For instance, 20190902.
        /// </summary>
        /// <example>20190902</example>
        public int Date { get; set; }

        /// <summary>
        /// The start time in 24-hour notation. 
        /// For instance, 930.
        /// </summary>
        /// <example>930</example>
        public int StartTime { get; set; }

        /// <summary>
        /// The end time in 24-hour notation. 
        /// For instance, 1015.
        /// </summary>
        /// <example>1015</example>
        public int EndTime { get; set; }

        /// <summary>
        /// The subject of this lesson.
        /// </summary>
        [JsonPropertyName("su")]
        public Subject Subject { get; set; }

        /// <summary>
        /// The lesson details. 
        /// An array with at least one element.
        /// </summary>
        public IEnumerable<LessonDetails> Details { get; set; } = Enumerable.Empty<LessonDetails>();
    }
}
