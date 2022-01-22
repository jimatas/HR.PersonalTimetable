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
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Institute Institute { get; set; }
        public Element Element { get; set; }
        public IEnumerable<Holiday> Holidays { get; set; }
        public IEnumerable<Lesson> Lessons { get; set; }
    }
}
