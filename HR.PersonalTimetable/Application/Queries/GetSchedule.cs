using HR.Common.Cqrs.Queries;
using HR.Common.Utilities;
using HR.PersonalTimetable.Application.Extensions;
using HR.PersonalTimetable.Application.Models;
using HR.PersonalTimetable.Application.Services;
using HR.WebUntisConnector;
using HR.WebUntisConnector.Configuration;
using HR.WebUntisConnector.Extensions;
using HR.WebUntisConnector.Model;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Application.Queries
{
    public class GetSchedule : IQuery<Schedule>, IValidatableObject
    {
        /// <summary>
        /// The RUAS institute name.
        /// Alternatively, a WebUntis school name may be supplied.
        /// </summary>
        [Required]
        [FromQuery(Name = "institute")]
        public string InstituteName { get; set; }

        [Required]
        [FromQuery(Name = "element")]
        public ElementType ElementType { get; set; }

        /// <summary>
        /// The primary key of the element in the WebUntis database.
        /// </summary>
        [FromQuery(Name = "id")]
        public int? ElementId { get; set; }

        /// <summary>
        /// The abbreviated name of the element.
        /// </summary>
        [FromQuery(Name = "name")]
        public string ElementName { get; set; }

        /// <summary>
        /// Optional start of the date range.
        /// Defaults to the first weekday of the current week.
        /// </summary>
        [FromQuery(Name = "start")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Optional end of the date range.
        /// Defaults to the last weekday (+1 day) of the current week.
        /// </summary>
        [FromQuery(Name = "end")]
        public DateTime? EndDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ElementId.IsNullOrDefault() && string.IsNullOrEmpty(ElementName))
            {
                yield return new ValidationResult(
                    errorMessage: $"Either the {nameof(ElementId)} field or the {nameof(ElementName)} field, or both must be specified.",
                    memberNames: new[] { nameof(ElementId), nameof(ElementName) });
            }
        }
    }

    public class GetScheduleHandler : IQueryHandler<GetSchedule, Schedule>
    {
        private readonly IApiClientFactory apiClientFactory;
        private readonly WebUntisConfigurationSection configuration;

        public GetScheduleHandler(ICachedApiClientFactory apiClientFactory, WebUntisConfigurationSection configuration)
        {
            this.apiClientFactory = Ensure.Argument.NotNull(() => apiClientFactory);
            this.configuration = Ensure.Argument.NotNull(() => configuration);
        }

        public async Task<Schedule> HandleAsync(GetSchedule query, CancellationToken cancellationToken)
        {
            var apiClient = await apiClientFactory.CreateApiClientAndLogInAsync(query.InstituteName, cancellationToken);
            try
            {
                return new Schedule
                {
                    StartDate = (DateTime)query.StartDate,
                    EndDate = (DateTime)query.EndDate,
                    Institute = GetInstitute(query.InstituteName),
                    Element = query.ElementType.CreateElement((int)query.ElementId, query.ElementName),
                    Lessons = await GetLessonsAsync(apiClient, query.ElementType, (int)query.ElementId, (DateTime)query.StartDate, (DateTime)query.EndDate, cancellationToken),
                    Holidays = await GetHolidaysAsync(apiClient, (DateTime)query.StartDate, (DateTime)query.EndDate, cancellationToken)
                };
            }
            finally
            {
                await apiClient.LogOutAsync(cancellationToken);
            }
        }

        private Institute GetInstitute(string instituteName)
        {
            var school = configuration.FindSchool(instituteName);
            var institute = school.Institutes.FirstOrDefault(institute => institute.Name.Equals(instituteName, StringComparison.OrdinalIgnoreCase)) ?? school.Institutes.First();
            return Institute.FromInstituteElement(institute);
        }

        private static async Task<IEnumerable<Holiday>> GetHolidaysAsync(IApiClient apiClient, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            return (await apiClient.GetHolidaysAsync(cancellationToken))
                .Where(holiday => holiday.ToDateTimeRange().Overlaps(new(startDate, endDate)));
        }

        private static async Task<IEnumerable<Lesson>> GetLessonsAsync(IApiClient apiClient, ElementType elementType, int elementId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            var (timetables, _) = await apiClient.GetTimetablesAsync(elementType, elementId, startDate, endDate, cancellationToken);
            var timegrids = await apiClient.GetTimegridsAsync(cancellationToken);
            var timetableGroups = timetables.ToTimetableGroups(timegrids);

            return timetableGroups.Select(timetableGroup => timetableGroup.ToLesson());
        }
    }
}
