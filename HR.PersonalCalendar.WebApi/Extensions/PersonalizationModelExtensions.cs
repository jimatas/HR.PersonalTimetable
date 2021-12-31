using HR.PersonalCalendar.WebApi.Models;
using HR.WebUntisConnector.Model;

using System;

namespace HR.PersonalCalendar.WebApi.Extensions
{
    public static class PersonalizationModelExtensions
    {
        public static Element ToElement(this PersonalizationModel personalizationModel)
        {
            return personalizationModel.ElementType switch
            {
                ElementType.Klasse => new Klasse { Id = (int)personalizationModel.ElementId, Name = personalizationModel.ElementName },
                ElementType.Teacher => new Teacher { Id = (int)personalizationModel.ElementId, Name = personalizationModel.ElementName },
                ElementType.Subject => new Subject { Id = (int)personalizationModel.ElementId, Name = personalizationModel.ElementName },
                ElementType.Room => new Room { Id = (int)personalizationModel.ElementId, Name = personalizationModel.ElementName },
                ElementType.Student => new Student { Id = (int)personalizationModel.ElementId, Name = personalizationModel.ElementName },
                _ => throw new ArgumentOutOfRangeException(nameof(personalizationModel), personalizationModel.ElementType, $"{nameof(personalizationModel)}.{nameof(ElementType)} is not a valid {nameof(ElementType)}."),
            };
        }
    }
}
