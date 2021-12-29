using HR.WebUntisConnector.Model;

using System;
using System.Collections.Generic;
using System.Linq;

namespace HR.PersonalCalendar.Extensions
{
    public static class TimetableGroupExtensions
    {
        /// <summary>
        /// Returns the distinct set of classes in the timetable group.
        /// </summary>
        /// <param name="group">A grouping of timetables to get the classes of.</param>
        /// <returns>The distinct set of classes gathered from the individual timetables in the timetable group.</returns>
        public static IEnumerable<Klasse> GetKlassen(this TimetableGroup group) 
            => new HashSet<Klasse>(group.Timetables.SelectMany(table => table.Klassen).OrderBy(klasse => klasse.Id));

        /// <summary>
        /// Returns the distinct set of teachers in the timetable group.
        /// </summary>
        /// <param name="group">A grouping of timetables to get the teachers of.</param>
        /// <returns>The distinct set of teachers gathered from the individual timetables in the timetable group.</returns>
        public static IEnumerable<Teacher> GetTeachers(this TimetableGroup group) 
            => new HashSet<Teacher>(group.Timetables.SelectMany(table => table.Teachers).OrderBy(teacher => teacher.Id));

        /// <summary>
        /// Returns the distinct set of subjects in the timetable group.
        /// </summary>
        /// <param name="group">A grouping of timetables to get the subjects of.</param>
        /// <returns>The distinct set of subjects gathered from the individual timetables in the timetable group.</returns>
        public static IEnumerable<Subject> GetSubjects(this TimetableGroup group) 
            => new HashSet<Subject>(group.Timetables.SelectMany(table => table.Subjects).OrderBy(subject => subject.Id));

        /// <summary>
        /// Returns the distinct set of classrooms in the timetable group.
        /// </summary>
        /// <param name="group">A grouping of timetables to get the classrooms of.</param>
        /// <returns>The distinct set of classrooms gathered from the individual timetables in the timetable group.</returns>
        public static IEnumerable<Room> GetRooms(this TimetableGroup group) 
            => new HashSet<Room>(group.Timetables.SelectMany(table => table.Rooms).OrderBy(room => room.Id));

        /// <summary>
        /// Returns the distinct set of lesson texts in the timetable group.
        /// </summary>
        /// <param name="group">A grouping of timetables to get the lesson texts of.</param>
        /// <returns>The distinct set of lesson texts gathered from the individual timetables in the timetable group.</returns>
        public static IEnumerable<string> GetLessonTexts(this TimetableGroup group) 
            => group.Timetables.Select(table => table.LessonText).Where(text => text != null).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(text => text).ToArray();
    }
}
