﻿using HR.PersonalCalendar.Model;

using System;
using System.Text.Json.Serialization;

namespace HR.PersonalCalendar.WebApi.Models
{
    public class PersonalizationReadModel : PersonalizationWriteModel
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PersonalizationReadModel"/> class with its properties initialized from the corresponding ones in the specified <see cref="PersonalTimetable"/> object.
        /// </summary>
        /// <param name="personalTimetable"></param>
        /// <returns></returns>
        public static PersonalizationReadModel FromPersonalTimetable(PersonalTimetable personalTimetable) => new()
        {
            Id = personalTimetable.Id,
            UserName = personalTimetable.UserName,
            InstituteName = personalTimetable.InstituteName,
            ElementType = personalTimetable.ElementType,
            ElementId = personalTimetable.ElementId,
            ElementName = personalTimetable.ElementName,
            IsVisible = personalTimetable.IsVisible
        };

        public Guid Id { get; set; }

        [JsonPropertyName("visible")]
        public bool IsVisible { get; set; } = true;
    }
}
