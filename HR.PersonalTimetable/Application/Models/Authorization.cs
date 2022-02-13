using HR.PersonalTimetable.Application.Exceptions;
using HR.PersonalTimetable.Application.Extensions;

using System;

namespace HR.PersonalTimetable.Application.Models
{
    public class Authorization
    {
        /// <summary>
        /// The client-provided hash that is to be verified.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The client's signing key.
        /// </summary>
        public SigningKey SigningKey { get; set; }

        /// <summary>
        /// The client-provided Unix timestamp in seconds
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// Verifies that the user whose hashed username was provided in the request has access to the timetable.
        /// </summary>
        /// <param name="timetable"></param>
        /// <returns><c>true</c> if access is granted.</returns>
        /// <exception cref="ForbiddenException">If access is denied.</exception>
        public bool VerifyAccess(PersonalTimetable timetable)
        {
            var hashToVerifyAgainst = string.Concat(timetable.UserName.ToLowerInvariant(), SigningKey, Timestamp).ToSha256();
            if (hashToVerifyAgainst.Equals(UserName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            throw new ForbiddenException($"User does not have access to {timetable}.");
        }

        /// <summary>
        /// Verifies that the user whose hashed username was provided in the request is allowed to create the timetable.
        /// </summary>
        /// <param name="timetable"></param>
        /// <returns><c>true</c> if access is granted.</returns>
        /// <exception cref="ForbiddenException">If access is denied.</exception>
        public bool VerifyCreateAccess(PersonalTimetable timetable)
        {
            try { return VerifyAccess(timetable); }
            catch (ForbiddenException) { throw new ForbiddenException($"Cannot create a {nameof(PersonalTimetable)} on behalf of another user."); }
        }
    }
}
