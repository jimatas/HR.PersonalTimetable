using HR.PersonalTimetable.Application.Models;
using HR.WebUntisConnector.Model;

using System.Collections.Generic;
using System.Linq;

namespace HR.PersonalTimetable.Application.Extensions
{
    public static class TimetableGroupExtensions
    {
        /// <summary>
        /// Converts a <see cref="TimetableGroup"/> object representing a grouping of timetables to a more succinct <see cref="Lesson"/> object.
        /// </summary>
        /// <param name="timetableGroup">The source <see cref="TimetableGroup"/> object.</param>
        /// <returns>The resulting <see cref="Lesson"/> object.</returns>
        public static Lesson ToLesson(this TimetableGroup timetableGroup)
        {
            var lessonDetails = new List<LessonDetails>();
            var lesson = new Lesson
            {
                LessonNumber = timetableGroup.LessonNumbers.First() / 100,
                Date = timetableGroup.Date,
                StartTime = timetableGroup.StartTime,
                EndTime = timetableGroup.EndTime,
                Details = lessonDetails
            };

            foreach (var lessonNumber in timetableGroup.LessonNumbers)
            {
                var timetable = timetableGroup.Timetables.First(timetable => timetable.LessonNumber == lessonNumber);

                lesson.Subject ??= timetable.Subjects.FirstOrDefault();
                lessonDetails.Add(new LessonDetails
                {
                    LessonNumber = lessonNumber,
                    LessonText = timetable.LessonText,
                    Teachers = timetable.Teachers,
                    Klassen = timetable.Klassen,
                    Rooms = timetable.Rooms
                });
            }

            return lesson;
        }
    }
}
