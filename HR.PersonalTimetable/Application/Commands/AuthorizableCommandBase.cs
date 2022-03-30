using HR.Common.Cqrs.Commands;
using HR.PersonalTimetable.Application.Models;

namespace HR.PersonalTimetable.Application.Commands
{
    public abstract class AuthorizableCommandBase : ICommand
    {
        /// <summary>
        /// The client-provided authorization data.
        /// </summary>
        protected internal Authorization Authorization { get; set; }
    }
}
