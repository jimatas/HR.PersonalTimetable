using HR.PersonalTimetable.Api.Models;
using HR.WebUntisConnector.Model;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HR.PersonalTimetable.Api.Extensions
{
    /// <summary>
    /// Defines some useful extension methods on the <see cref="Lesson"/> class, such as for exporting to iCalendar (*.ics) format.
    /// </summary>
    public static class LessonExtensions
    {
        /// <summary>
        /// Returns a <see cref="DateTime"/> instance that represents the date and start time of the lesson.
        /// </summary>
        /// <param name="lesson"></param>
        /// <returns></returns>
        public static DateTime GetStartDateTime(this Lesson lesson)
            => DateTime.ParseExact(lesson.Date.ToString() + lesson.StartTime.ToString("0000"), "yyyyMMddHHmm", CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns a <see cref="DateTime"/> instance that represents the date and end time of the lesson.
        /// </summary>
        /// <param name="lesson"></param>
        /// <returns></returns>
        public static DateTime GetEndDateTime(this Lesson lesson)
            => DateTime.ParseExact(lesson.Date.ToString() + lesson.EndTime.ToString("0000"), "yyyyMMddHHmm", CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns the distinct set of classrooms associated with the lesson.
        /// </summary>
        /// <param name="lesson"></param>
        /// <returns>The distinct set of classrooms, pulled from the individual lesson details.</returns>
        public static IEnumerable<Room> GetRooms(this Lesson lesson)
            => new HashSet<Room>(lesson.Details.SelectMany(details => details.Rooms).OrderBy(room => room.Id));

        /// <summary>
        /// Export a range of lessons to iCalendar (*.ics) format.
        /// </summary>
        /// <param name="lessons">The timetable data to export.</param>
        /// <param name="refreshInterval">The suggested refresh interval in minutes. Default value is half an hour.</param>
        /// <returns>A string containing the exported data in iCalendar format.</returns>
        public static string ExportCalendar(this IEnumerable<Lesson> lessons, int refreshInterval = 30)
        {
            var calendarBuilder = new StringBuilder();

            calendarBuilder.AppendLine("BEGIN:VCALENDAR");
            calendarBuilder.AppendLine("VERSION:2.0");
            calendarBuilder.AppendLine("PRODID:-//Hogeschool Rotterdam//HR.WebUntisConnector.ApiClient//NL");
            calendarBuilder.AppendLine("CALSCALE:GREGORIAN");
            calendarBuilder.AppendLine("X-WR-CALNAME:HR Rooster");
            calendarBuilder.AppendFormat("X-PUBLISHED-TTL;VALUE=DURATION:PT{0}M", refreshInterval).AppendLine();
            calendarBuilder.AppendFormat("REFRESH-INTERVAL;VALUE=DURATION:PT{0}M", refreshInterval).AppendLine();

            foreach (var lesson in lessons)
            {
                lesson.AppendTo(calendarBuilder);
            }

            calendarBuilder.AppendLine("END:VCALENDAR");

            return calendarBuilder.ToString();
        }

        private static void AppendTo(this Lesson lesson, StringBuilder calendarBuilder)
        {
            calendarBuilder.AppendLine("BEGIN:VEVENT");

            var lessonNumber = lesson.LessonNumber;
            calendarBuilder.Append("UID:").AppendLine($"{lesson.Date}T{lesson.StartTime:0000}-LS{lessonNumber}@webuntis.hr.nl");

            calendarBuilder.Append("DTSTAMP:").AppendLine(DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ"));
            calendarBuilder.Append("DTSTART:").AppendLine(lesson.GetStartDateTime().ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));
            calendarBuilder.Append("DTEND:").AppendLine(lesson.GetEndDateTime().ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));

            var rooms = lesson.GetRooms();
            if (rooms.Any())
            {
                calendarBuilder.Append("LOCATION:").AppendLine(string.Join(", ", rooms.Select(room => room.Name)).EscapePropertyValue());
            }

            calendarBuilder.Append("SUMMARY:").AppendLine((lesson.Subject?.LongName ?? lesson.Subject?.Name ?? $"Les {lessonNumber}").EscapePropertyValue());

            var descriptionBuilder = new StringBuilder();
            foreach (var details in lesson.Details)
            {
                if (descriptionBuilder.Length != 0)
                {
                    descriptionBuilder.Append("\\n");
                }
                details.AppendTo(descriptionBuilder);
            }
            calendarBuilder.Append("DESCRIPTION:").AppendLine(descriptionBuilder.ToString().EscapePropertyValue());

            calendarBuilder.AppendLine("TRANSP:OPAQUE");
            calendarBuilder.AppendLine("X-MICROSOFT-CDO-INTENDEDSTATUS:BUSY");
            calendarBuilder.AppendLine("END:VEVENT");
        }

        private static void AppendTo(this LessonDetails details, StringBuilder descriptionBuilder)
        {
            descriptionBuilder.Append("Klas: ").Append(string.Join(", ", details.Klassen.Select(klasse => klasse.Name))).Append("\\n");
            descriptionBuilder.Append("Docent: ").Append(string.Join(", ", details.Teachers.Select(teacher => teacher.Name))).Append("\\n");
            descriptionBuilder.Append("Locatie: ").Append(string.Join(", ", details.Rooms.Select(room => room.Name))).Append("\\n");
            if (!string.IsNullOrEmpty(details.LessonText))
            {
                descriptionBuilder.Append("Opmerkingen: ").Append(details.LessonText).Append("\\n");
            }
        }

        private static string EscapePropertyValue(this string value)
        {
            foreach (var illegalChar in new[] { ';', ',', '\n' })
            {
                if (value.Contains(illegalChar))
                {
                    var pattern = string.Format("(?<!\\\\){0}", Regex.Escape(illegalChar.ToString()));
                    var escaped = string.Format("\\{0}", illegalChar);

                    value = Regex.Replace(value, pattern, escaped);
                }
            }

            if (value.Contains('"'))
            {
                value = value.Replace("\"", string.Empty);
            }

            return value;
        }
    }
}
