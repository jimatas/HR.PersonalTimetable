using HR.PersonalCalendar.WebApi.Models;
using HR.WebUntisConnector.Model;

using System;
using System.ComponentModel;

namespace HR.PersonalCalendar.WebApi.Extensions
{
    public static class PersonalizationModelExtensions
    {
        public static Element ToElement(this PersonalizationModel personalizationModel)
        {
            return personalizationModel.ElementType switch
            {
                ElementType.Klasse => new Klasse { Id = personalizationModel.ElementId, Name = personalizationModel.ElementName },
                ElementType.Teacher => new Teacher { Id = personalizationModel.ElementId, Name = personalizationModel.ElementName },
                ElementType.Subject => new Subject { Id = personalizationModel.ElementId, Name = personalizationModel.ElementName },
                ElementType.Room => new Room { Id = personalizationModel.ElementId, Name = personalizationModel.ElementName },
                ElementType.Student => new Student { Id = personalizationModel.ElementId, Name = personalizationModel.ElementName },
                _ => throw new InvalidEnumArgumentException($"{nameof(personalizationModel)}.{nameof(personalizationModel.ElementType)}", Convert.ToInt32(personalizationModel.ElementType), typeof(ElementType))
            };
        }
    }
}
