using HR.PersonalCalendar.WebApi.Models;
using HR.WebUntisConnector.Model;

using System;
using System.ComponentModel;

namespace HR.PersonalCalendar.WebApi.Extensions
{
    public static class PersonalizationExtensions
    {
        public static Element ToElement(this Personalization personalization)
        {
            return personalization.ElementType switch
            {
                ElementType.Klasse => new Klasse { Id = (int)personalization.ElementId, Name = personalization.ElementName },
                ElementType.Teacher => new Teacher { Id = (int)personalization.ElementId, Name = personalization.ElementName },
                ElementType.Subject => new Subject { Id = (int)personalization.ElementId, Name = personalization.ElementName },
                ElementType.Room => new Room { Id = (int)personalization.ElementId, Name = personalization.ElementName },
                ElementType.Student => new Student { Id = (int)personalization.ElementId, Name = personalization.ElementName },
                _ => throw new InvalidEnumArgumentException($"{nameof(personalization)}.{nameof(personalization.ElementType)}", Convert.ToInt32(personalization.ElementType), typeof(ElementType))
            };
        }
    }
}
