using HR.WebUntisConnector.Extensions;
using HR.WebUntisConnector.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HR.PersonalCalendar.Api.Extensions
{
    /// <summary>
    /// Useful extension methods on <see cref="TimetableGroup"/> class, such as that for exporting to iCalendar (*.ics) format.
    /// </summary>
    public static class TimetableGroupExtensions
    {
        /// <summary>
        /// Returns the distinct set of classes in the timetable group.
        /// </summary>
        /// <param name="timetableGroup">A grouping of timetables to get the classes of.</param>
        /// <returns>The distinct set of classes gathered from the individual timetables in the timetable group.</returns>
        public static IEnumerable<Klasse> GetKlassen(this TimetableGroup timetableGroup)
            => new HashSet<Klasse>(timetableGroup.Timetables.SelectMany(table => table.Klassen).OrderBy(klasse => klasse.Id));

        /// <summary>
        /// Returns the distinct set of teachers in the timetable group.
        /// </summary>
        /// <param name="timetableGroup">A grouping of timetables to get the teachers of.</param>
        /// <returns>The distinct set of teachers gathered from the individual timetables in the timetable group.</returns>
        public static IEnumerable<Teacher> GetTeachers(this TimetableGroup timetableGroup)
            => new HashSet<Teacher>(timetableGroup.Timetables.SelectMany(table => table.Teachers).OrderBy(teacher => teacher.Id));

        /// <summary>
        /// Returns the distinct set of subjects in the timetable group.
        /// </summary>
        /// <param name="timetableGroup">A grouping of timetables to get the subjects of.</param>
        /// <returns>The distinct set of subjects gathered from the individual timetables in the timetable group.</returns>
        public static IEnumerable<Subject> GetSubjects(this TimetableGroup timetableGroup)
            => new HashSet<Subject>(timetableGroup.Timetables.SelectMany(table => table.Subjects).OrderBy(subject => subject.Id));

        /// <summary>
        /// Returns the distinct set of classrooms in the timetable group.
        /// </summary>
        /// <param name="timetableGroup">A grouping of timetables to get the classrooms of.</param>
        /// <returns>The distinct set of classrooms gathered from the individual timetables in the timetable group.</returns>
        public static IEnumerable<Room> GetRooms(this TimetableGroup timetableGroup)
            => new HashSet<Room>(timetableGroup.Timetables.SelectMany(table => table.Rooms).OrderBy(room => room.Id));

        /// <summary>
        /// Returns the distinct set of lesson texts in the timetable group.
        /// </summary>
        /// <param name="timetableGroup">A grouping of timetables to get the lesson texts of.</param>
        /// <returns>The distinct set of lesson texts gathered from the individual timetables in the timetable group.</returns>
        public static IEnumerable<string> GetLessonTexts(this TimetableGroup timetableGroup)
            => timetableGroup.Timetables.Select(table => table.LessonText).Where(text => !string.IsNullOrEmpty(text)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(text => text).ToArray();

        /// <summary>
        /// Export a range of timetables to iCalendar (*.ics) format.
        /// </summary>
        /// <param name="timetableGroups">The timetable data to export.</param>
        /// <param name="refreshInterval">The suggested refresh interval in minutes. Default value is half an hour.</param>
        /// <returns>A string containing the exported data in iCalendar format.</returns>
        public static string ExportCalendar(this IEnumerable<TimetableGroup> timetableGroups, int refreshInterval = 30)
        {
            var calendarBuilder = new StringBuilder();

            calendarBuilder.AppendLine("BEGIN:VCALENDAR");
            calendarBuilder.AppendLine("VERSION:2.0");
            calendarBuilder.AppendLine("PRODID:-//Hogeschool Rotterdam//HR.WebUntisConnector.ApiClient//NL");
            calendarBuilder.AppendLine("CALSCALE:GREGORIAN");
            calendarBuilder.AppendLine("X-WR-CALNAME:HR Rooster");
            calendarBuilder.AppendFormat("X-PUBLISHED-TTL;VALUE=DURATION:PT{0}M", refreshInterval).AppendLine();
            calendarBuilder.AppendFormat("REFRESH-INTERVAL;VALUE=DURATION:PT{0}M", refreshInterval).AppendLine();

            foreach (var timetableGroup in timetableGroups)
            {
                timetableGroup.AppendTo(calendarBuilder);
            }

            calendarBuilder.AppendLine("END:VCALENDAR");

            return calendarBuilder.ToString();
        }

        private static void AppendTo(this TimetableGroup timetableGroup, StringBuilder calendarBuilder)
        {
            calendarBuilder.AppendLine("BEGIN:VEVENT");

            var lessonNumber = timetableGroup.LessonNumbers.First() / 100;
            calendarBuilder.Append("UID:").AppendLine($"{timetableGroup.Date}T{timetableGroup.StartTime:0000}-LS{lessonNumber}@webuntis.hr.nl");

            calendarBuilder.Append("DTSTAMP:").AppendLine(DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ"));
            calendarBuilder.Append("DTSTART:").AppendLine(timetableGroup.GetStartDateTime().ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));
            calendarBuilder.Append("DTEND:").AppendLine(timetableGroup.GetEndDateTime().ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));

            var rooms = timetableGroup.GetRooms();
            if (rooms.Any())
            {
                calendarBuilder.Append("LOCATION:").AppendLine(string.Join(", ", rooms.Select(room => room.Name)).EscapePropertyValue());
            }

            var lessonInfo = Convert.ToString(lessonNumber);
            var lessonTexts = timetableGroup.GetLessonTexts();
            if (lessonTexts.Any())
            {
                lessonInfo += " - " + string.Join(", ", lessonTexts);
            }

            var subjects = timetableGroup.GetSubjects();
            calendarBuilder.Append("SUMMARY:").AppendLine((subjects.FirstOrDefault()?.LongName ?? subjects.FirstOrDefault()?.Name ?? lessonInfo).EscapePropertyValue());

            var descriptionBuilder = new StringBuilder();
            var teachers = timetableGroup.GetTeachers();
            if (teachers.Any())
            {
                descriptionBuilder.Append("Docent: ").Append(string.Join(", ", teachers.Select(teacher => teacher.Name))).Append("\\n");
            }

            var klassen = timetableGroup.GetKlassen();
            if (klassen.Any())
            {
                descriptionBuilder.Append("Klas: ").Append(string.Join(", ", klassen.Select(klasse => klasse.Name))).Append("\\n");
            }
            descriptionBuilder.Append("Les: ").Append(lessonInfo);
            calendarBuilder.Append("DESCRIPTION:").AppendLine(descriptionBuilder.ToString().EscapePropertyValue());

            calendarBuilder.AppendLine("TRANSP:OPAQUE");
            calendarBuilder.AppendLine("X-MICROSOFT-CDO-INTENDEDSTATUS:BUSY");
            calendarBuilder.AppendLine("END:VEVENT");
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
