using HR.WebUntisConnector.Model;

using System;
using System.ComponentModel;

namespace HR.PersonalTimetable.Application.Extensions
{
    public static class ElementTypeExtensions
    {
        /// <summary>
        /// Creates an <see cref="Element"/> of the appropriate type (i.e., <see cref="Klasse"/>, <see cref="Teacher"/>, <see cref="Subject"/>, <see cref="Room"/>, <see cref="Student"/>) with the specified id and name.
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="id">The primary key of the element in the WebUntis database.</param>
        /// <param name="name">The abbreviated name of the element.</param>
        /// <returns></returns>
        public static Element CreateElement(this ElementType elementType, int id, string name) => elementType switch
        {
            ElementType.Klasse => new Klasse { Id = id, Name = name },
            ElementType.Teacher => new Teacher { Id = id, Name = name },
            ElementType.Subject => new Subject { Id = id, Name = name },
            ElementType.Room => new Room { Id = id, Name = name },
            ElementType.Student => new Student { Id = id, Name = name },
            _ => throw new InvalidEnumArgumentException(nameof(elementType), Convert.ToInt32(elementType), typeof(ElementType))
        };
    }
}
