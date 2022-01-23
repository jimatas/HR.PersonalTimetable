using HR.WebUntisConnector.Model;

using System;
using System.Collections.Generic;

namespace HR.PersonalTimetable.Api.Models
{
    /// <summary>
    /// Complete lesson schedule for a particular timetable element, time period and institute.
    /// Includes any holidays that occur in that period as well.
    /// </summary>
    public class Schedule
    {
        /// <example>2022-01-17T00:00:00.000Z</example>
        public DateTime StartDate { get; set; }
        
        /// <example>2022-01-21T00:00:00.000Z</example>
        public DateTime EndDate { get; set; }

        public Institute Institute { get; set; }
        public Element Element { get; set; }
        public IEnumerable<Holiday> Holidays { get; set; }
        public IEnumerable<Lesson> Lessons { get; set; }
    }
}
