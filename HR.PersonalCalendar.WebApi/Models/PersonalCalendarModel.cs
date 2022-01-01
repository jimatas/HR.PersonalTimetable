using HR.WebUntisConnector.Model;

using System;
using System.Collections.Generic;

namespace HR.PersonalCalendar.WebApi.Models
{
    public class PersonalCalendarModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public InstituteModel Institute { get; set; }
        public Element Element { get; set; }
        public IEnumerable<Holiday> Holidays { get; set; }
        public IEnumerable<TimetableGroup> TimetableGroups { get; set; }
    }
}
