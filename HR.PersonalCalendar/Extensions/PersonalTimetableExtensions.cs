using HR.PersonalCalendar.Model;

using System;

namespace HR.PersonalCalendar.Extensions
{
    public static class PersonalTimetableExtensions
    {
        /// <summary>
        /// Ensures that the user with the specified username has access to the timetable by verifying that the usernames match.
        /// Returns <c>true</c> if access is granted, otherwise throws an <see cref="UnauthorizedException"/>.
        /// </summary>
        /// <param name="personalTimetable"></param>
        /// <param name="userNameToVerify"></param>
        /// <returns></returns>
        public static bool EnsureHasAccess(this PersonalTimetable personalTimetable, string userNameToVerify)
            => personalTimetable.UserName.Equals(userNameToVerify, StringComparison.InvariantCultureIgnoreCase) ? true : throw new UnauthorizedException($"User \"{userNameToVerify}\" does not have access to the timetable with ID \"{personalTimetable.Id}\".");
    }
}
