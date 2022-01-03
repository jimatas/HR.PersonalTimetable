using HR.WebUntisConnector.Model;

using System;

namespace HR.PersonalCalendar.Api.Models
{
    /// <summary>
    /// Thrown to indicate that some specified <see cref="Element"/> does not exist or could not be found.
    /// </summary>
    public class NoSuchElementException : Exception
    {
        public NoSuchElementException(ElementType elementType, int elementId)
            : base(message: $"{elementType} with {nameof(Element.Id)} {elementId}") { }

        public NoSuchElementException(ElementType elementType, string elementName)
            : base(message: $"{elementType} with {nameof(Element.Name)} {elementName}") { }
    }
}
