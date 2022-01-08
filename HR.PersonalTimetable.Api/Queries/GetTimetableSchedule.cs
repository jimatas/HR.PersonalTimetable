using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Extensions;
using HR.PersonalTimetable.Api.Infrastructure;
using HR.PersonalTimetable.Api.Models;
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

namespace HR.PersonalTimetable.Api.Queries
{
    public class GetTimetableSchedule : IQuery<TimetableSchedule>, IValidatableObject
    {
        [Required]
        [FromQuery(Name = "institute")]
        public string InstituteName { get; set; }

        [Required]
        [FromQuery(Name = "element")]
        public ElementType ElementType { get; set; }

        [FromQuery(Name = "id")]
        public int? ElementId { get; set; }

        [FromQuery(Name = "name")]
        public string ElementName { get; set; }

        [FromQuery(Name = "start")]
        public DateTime? StartDate { get; set; }

        [FromQuery(Name = "end")]
        public DateTime? EndDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ElementId.IsNullOrDefault() && string.IsNullOrEmpty(ElementName))
            {
                var errorMessage = $"Either the {nameof(ElementId)} field or the {nameof(ElementName)} field, or both must be specified.";
                var memberNames = new[] { nameof(ElementId), nameof(ElementName) };
                return new ValidationResult[] { new(errorMessage, memberNames) };
            }
            return Enumerable.Empty<ValidationResult>();
        }
    }

    public class GetTimetableScheduleHandler : IQueryHandler<GetTimetableSchedule, TimetableSchedule>
    {
        private readonly IApiClientFactory apiClientFactory;
        private readonly WebUntisConfigurationSection configuration;

        public GetTimetableScheduleHandler(ICachedApiClientFactory apiClientFactory, WebUntisConfigurationSection configuration)
        {
            this.apiClientFactory = Ensure.Argument.NotNull(() => apiClientFactory);
            this.configuration = Ensure.Argument.NotNull(() => configuration);
        }

        public async Task<TimetableSchedule> HandleAsync(GetTimetableSchedule query, CancellationToken cancellationToken)
        {
            var apiClient = await apiClientFactory.CreateApiClientAndLogInAsync(query.InstituteName, cancellationToken).ConfigureAwait(false);
            try
            {
                return new TimetableSchedule
                {
                    StartDate = (DateTime)query.StartDate,
                    EndDate = (DateTime)query.EndDate,
                    Institute = GetInstitute(query.InstituteName),
                    Element = query.ElementType.CreateElement((int)query.ElementId, query.ElementName),
                    TimetableGroups = await GetTimetableGroupsAsync(apiClient, query.ElementType, (int)query.ElementId, (DateTime)query.StartDate, (DateTime)query.EndDate, cancellationToken).ConfigureAwait(false),
                    Holidays = await GetHolidaysAsync(apiClient, (DateTime)query.StartDate, (DateTime)query.EndDate, cancellationToken).ConfigureAwait(false)
                };
            }
            finally
            {
                await apiClient.LogOutAsync(cancellationToken).ConfigureAwait(false);
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
            return (await apiClient.GetHolidaysAsync(cancellationToken).ConfigureAwait(false))
                .Where(holiday => holiday.ToDateTimeRange().Overlaps(new(startDate, endDate)));
        }

        private static async Task<IEnumerable<TimetableGroup>> GetTimetableGroupsAsync(IApiClient apiClient, ElementType elementType, int elementId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            var (timetables, _) = await apiClient.GetTimetablesAsync(elementType, elementId, startDate, endDate, cancellationToken).ConfigureAwait(false);
            var timegrids = await apiClient.GetTimegridsAsync(cancellationToken).ConfigureAwait(false);
            return timetables.ToTimetableGroups(timegrids);
        }
    }
}
