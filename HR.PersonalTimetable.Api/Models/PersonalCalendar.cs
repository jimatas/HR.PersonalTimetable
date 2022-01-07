using HR.WebUntisConnector.Model;

using System;
using System.Collections.Generic;

namespace HR.PersonalTimetable.Api.Models
{
    public class PersonalCalendar
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Institute Institute { get; set; }
        public Element Element { get; set; }
        public IEnumerable<Holiday> Holidays { get; set; }
        public IEnumerable<TimetableGroup> TimetableGroups { get; set; }
    }
}
