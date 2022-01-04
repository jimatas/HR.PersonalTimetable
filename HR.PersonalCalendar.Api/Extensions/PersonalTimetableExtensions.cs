using HR.PersonalCalendar.Api.Models;

using System;

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
            return personalTimetable.UserName.Equals(userNameToVerify, StringComparison.InvariantCultureIgnoreCase) ? true :
                throw new UnauthorizedException($"User {userNameToVerify} cannot create a {nameof(PersonalTimetable)} on behalf of another user.");
        }
    }
}
