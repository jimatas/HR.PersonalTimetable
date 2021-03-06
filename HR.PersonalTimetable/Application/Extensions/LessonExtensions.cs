using HR.PersonalTimetable.Application.Models;
using HR.PersonalTimetable.Application.Services;
using HR.WebUntisConnector.Model;

using Microsoft.Extensions.Localization;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace HR.PersonalTimetable.Application.Extensions
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
        /// <param name="refreshInterval">The suggested refresh interval.</param>
        /// <param name="clock">The system clock abstraction.</param>
        /// <param name="localizer">The localization provider to use.</param>
        /// <returns>A string containing the exported data in iCalendar format.</returns>
        public static string ExportCalendar(this IEnumerable<Lesson> lessons, TimeSpan refreshInterval, IClock clock, IStringLocalizer localizer)
        {
            var calendarBuilder = new StringBuilder();

            calendarBuilder.AppendLine("BEGIN:VCALENDAR");
            calendarBuilder.AppendLine("VERSION:2.0");
            calendarBuilder.Append("PRODID:").AppendLine(localizer["ProductId"]);
            calendarBuilder.AppendLine("CALSCALE:GREGORIAN");
            calendarBuilder.Append("X-WR-CALNAME:").AppendLine(localizer["CalendarName"]);
            calendarBuilder.Append("X-PUBLISHED-TTL;VALUE=DURATION:").AppendLine(XmlConvert.ToString(refreshInterval));
            calendarBuilder.Append("REFRESH-INTERVAL;VALUE=DURATION:").AppendLine(XmlConvert.ToString(refreshInterval));

            foreach (var lesson in lessons)
            {
                lesson.AppendTo(calendarBuilder, clock, localizer);
            }

            calendarBuilder.AppendLine("END:VCALENDAR");

            return calendarBuilder.ToString();
        }

        private static void AppendTo(this Lesson lesson, StringBuilder calendarBuilder, IClock clock, IStringLocalizer localizer)
        {
            calendarBuilder.AppendLine("BEGIN:VEVENT");

            var lessonNumber = lesson.LessonNumber;
            calendarBuilder.Append("UID:").AppendLine($"{lesson.Date}T{lesson.StartTime:0000}-LS{lessonNumber}@webuntis.hr.nl");

            calendarBuilder.Append("DTSTAMP:").AppendLine(clock.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'"));
            calendarBuilder.Append("DTSTART:").AppendLine(lesson.GetStartDateTime().ToUniversalTime().ToString("yyyyMMdd'T'HHmmss'Z'"));
            calendarBuilder.Append("DTEND:").AppendLine(lesson.GetEndDateTime().ToUniversalTime().ToString("yyyyMMdd'T'HHmmss'Z'"));

            var rooms = lesson.GetRooms();
            if (rooms.Any())
            {
                calendarBuilder.Append("LOCATION:").AppendLine(string.Join(", ", rooms.Select(room => room.Name)).EscapePropertyValue());
            }

            calendarBuilder.Append("SUMMARY:").AppendLine((lesson.Subject?.LongName ?? lesson.Subject?.Name ?? $"{localizer["Lesson"]} {lessonNumber}").EscapePropertyValue());

            var descriptionBuilder = new StringBuilder();
            foreach (var details in lesson.Details)
            {
                if (descriptionBuilder.Length != 0)
                {
                    descriptionBuilder.Append("\\n");
                }
                details.AppendTo(descriptionBuilder, localizer);
            }
            calendarBuilder.Append("DESCRIPTION:").AppendLine(descriptionBuilder.ToString().EscapePropertyValue());

            calendarBuilder.AppendLine("TRANSP:OPAQUE");
            calendarBuilder.AppendLine("X-MICROSOFT-CDO-INTENDEDSTATUS:BUSY");
            calendarBuilder.AppendLine("END:VEVENT");
        }

        private static void AppendTo(this LessonDetails details, StringBuilder descriptionBuilder, IStringLocalizer localizer)
        {
            descriptionBuilder.Append(localizer["Class"]).Append(": ").Append(string.Join(", ", details.Klassen.Select(klasse => klasse.Name))).Append("\\n");
            descriptionBuilder.Append(localizer["Instructor"]).Append(": ").Append(string.Join(", ", details.Teachers.Select(teacher => teacher.Name))).Append("\\n");
            descriptionBuilder.Append(localizer["Location"]).Append(": ").Append(string.Join(", ", details.Rooms.Select(room => room.Name))).Append("\\n");
            if (!string.IsNullOrEmpty(details.LessonText))
            {
                descriptionBuilder.Append(localizer["Remark"]).Append(": ").Append(details.LessonText).Append("\\n");
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
