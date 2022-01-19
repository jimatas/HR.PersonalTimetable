using Developist.Core.Persistence.Entities;

using HR.PersonalTimetable.Api.Extensions;
using HR.WebUntisConnector.Model;

using System;
using System.Text.Json.Serialization;

namespace HR.PersonalTimetable.Api.Models
{
    /// <summary>
    /// Represents the personal timetable of a user.
    /// </summary>
    public class PersonalTimetable : EntityBase<Guid>
    {
        public PersonalTimetable() { }
        public PersonalTimetable(Guid id) => Id = id;

        [JsonIgnore]
        public override bool IsTransient => base.IsTransient;

        /// <summary>
        /// The integration through which this personal timetable was created.
        /// </summary>
        [JsonIgnore]
        public Integration Integration { get; set; }

        /// <summary>
        /// Depending on whether the user is a student or an employee, either their student number or their employee code.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The RUAS institute that the timetable element falls under.
        /// </summary>
        public string InstituteName { get; set; }

        /// <summary>
        /// The type of the element.
        /// </summary>
        /// <seealso cref="Element.Type"/>
        public ElementType ElementType { get; set; }

        /// <summary>
        /// The primary key of the element in the WebUntis database.
        /// </summary>
        /// <seealso cref="Element.Id"/>
        public int ElementId { get; set; }

        /// <summary>
        /// The unique name of the element. 
        /// Provides an alternative way of identifiying elements.
        /// </summary>
        /// <seealso cref="Element.Name"/>
        public string ElementName { get; set; }

        /// <summary>
        /// If <see cref="ElementType"/> is <see cref="ElementType.Klasse"/>, the ID of the school year that it is defined in.
        /// </summary>
        public int? SchoolYearId { get; set; }

        /// <summary>
        /// Allows the user to temporarily hide a timetable without having to delete it from their personal timetable.
        /// Default value is <c>true</c>.
        /// </summary>
        [JsonPropertyName("visible")]
        public bool IsVisible { get; set; } = true;

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateLastModified { get; set; }

        /// <summary>
        /// Verifies that the user whose hashed username was provided in the request has access to this timetable.
        /// </summary>
        /// <param name="userNameToVerify"></param>
        /// <param name="secret"></param>
        /// <returns><c>true</c> if access is granted.</returns>
        /// <exception cref="UnauthorizedException">If access is denied.</exception>
        public bool VerifyAccess(string userNameToVerify, SigningKey secret)
        {
            return UserName.ToLower().ToSha256(secret.Key).Equals(userNameToVerify, StringComparison.OrdinalIgnoreCase) ? true
                : throw new UnauthorizedException($"User does not have access to {this}.");
        }

        /// <summary>
        /// Verifies that the user whose hashed username was provided in the request is allowed to create the timetable.
        /// </summary>
        /// <param name="userNameToVerify"></param>
        /// <param name="secret"></param>
        /// <returns><c>true</c> if access is granted.</returns>
        /// <exception cref="UnauthorizedException">If access is denied.</exception>
        public bool VerifyCreateAccess(string userNameToVerify, SigningKey secret)
        {
            try { return VerifyAccess(userNameToVerify, secret); }
            catch (UnauthorizedException) { throw new UnauthorizedException($"Cannot create a {nameof(PersonalTimetable)} on behalf of another user."); }
        }
    }
}
