using HR.PersonalCalendar.Api.Models;
using HR.WebUntisConnector.Model;

using System;
using System.ComponentModel;

namespace HR.PersonalCalendar.Api.Extensions
{
    public static class PersonalTimetableExtensions
    {
        /// <summary>
        /// Verifies that the user with the specified username may actually create the timetable.
        /// </summary>
        /// <param name="personalTimetable"></param>
        /// <param name="userNameToVerify"></param>
        /// <returns></returns>
        public static bool VerifyCreateAccess(this PersonalTimetable personalTimetable, string userNameToVerify)
        {
            return personalTimetable.UserName.Equals(userNameToVerify, StringComparison.OrdinalIgnoreCase) ? true :
                throw new UnauthorizedException($"User {userNameToVerify} cannot create a {nameof(PersonalTimetable)} on behalf of another user.");
        }

        /// <summary>
        /// Returns an <see cref="Element"/> that represents the timetable elements returned in this personal timetable.
        /// </summary>
        /// <param name="personalTimetable"></param>
        /// <returns></returns>
        public static Element ToElement(this PersonalTimetable personalTimetable)
        {
            return personalTimetable.ElementType switch
            {
                ElementType.Klasse => new Klasse { Id = personalTimetable.ElementId, Name = personalTimetable.ElementName },
                ElementType.Teacher => new Teacher { Id = personalTimetable.ElementId, Name = personalTimetable.ElementName },
                ElementType.Subject => new Subject { Id = personalTimetable.ElementId, Name = personalTimetable.ElementName },
                ElementType.Room => new Room { Id = personalTimetable.ElementId, Name = personalTimetable.ElementName },
                ElementType.Student => new Student { Id = personalTimetable.ElementId, Name = personalTimetable.ElementName },
                _ => throw new InvalidEnumArgumentException($"{nameof(personalTimetable)}.{nameof(personalTimetable.ElementType)}", Convert.ToInt32(personalTimetable.ElementType), typeof(ElementType))
            };
        }
    }
}
