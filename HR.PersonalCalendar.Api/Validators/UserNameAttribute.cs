using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace HR.PersonalCalendar.Api.Validators
{
    public class UserNameAttribute : ValidationAttribute
    {
        public UserNameAttribute() : this("The {0} field must be either a student number or an employee code.") { }
        public UserNameAttribute(string errorMessage) : base(errorMessage) { }

        public override bool IsValid(object value)
        {
            return value is string userName && Regex.IsMatch(userName, "[0-9]{7,8}|[a-zA-Z]{5}");
        }
    }
}
