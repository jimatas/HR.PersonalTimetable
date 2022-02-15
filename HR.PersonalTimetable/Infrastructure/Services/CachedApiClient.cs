using Developist.Core.Utilities;

using HR.PersonalTimetable.Application.Exceptions;
using HR.WebUntisConnector;
using HR.WebUntisConnector.Infrastructure;
using HR.WebUntisConnector.Model;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Infrastructure.Services
{
    public class CachedApiClient : IApiClient
    {
        private readonly IApiClient apiClient;
        private readonly string schoolOrInstituteName;

        public CachedApiClient(IApiClient apiClient, string schoolOrInstituteName)
        {
            this.apiClient = Ensure.Argument.NotNull(() => apiClient);
            this.schoolOrInstituteName = Ensure.Argument.NotNullOrEmpty(() => schoolOrInstituteName);
        }

        public bool IsAuthenticated => apiClient.IsAuthenticated;

        public Task<SchoolYear> GetCurrentSchoolYearAsync(CancellationToken cancellationToken = default)
            => apiClient.GetCurrentSchoolYearAsync(cancellationToken);

        public Task<IEnumerable<Department>> GetDepartmentsAsync(CancellationToken cancellationToken = default)
            => apiClient.GetDepartmentsAsync(cancellationToken);

        public Task<IEnumerable<Holiday>> GetHolidaysAsync(CancellationToken cancellationToken = default)
            => apiClient.GetHolidaysAsync(cancellationToken);

        public Task<IEnumerable<Klasse>> GetKlassenAsync(CancellationToken cancellationToken = default)
            => apiClient.GetKlassenAsync(cancellationToken);

        public Task<IEnumerable<Klasse>> GetKlassenAsync(KlasseParameters parameters, CancellationToken cancellationToken = default)
            => apiClient.GetKlassenAsync(parameters, cancellationToken);

        public Task<DateTime> GetLatestImportTimeAsync(CancellationToken cancellationToken = default)
            => apiClient.GetLatestImportTimeAsync(cancellationToken);

        public Task<IEnumerable<Room>> GetRoomsAsync(CancellationToken cancellationToken = default)
            => apiClient.GetRoomsAsync(cancellationToken);

        public Task<IEnumerable<SchoolYear>> GetSchoolYearsAsync(CancellationToken cancellationToken = default)
            => apiClient.GetSchoolYearsAsync(cancellationToken);

        public Task<IEnumerable<Student>> GetStudentsAsync(CancellationToken cancellationToken = default)
            => apiClient.GetStudentsAsync(cancellationToken);

        public Task<IEnumerable<Subject>> GetSubjectsAsync(CancellationToken cancellationToken = default)
            => apiClient.GetSubjectsAsync(cancellationToken);

        public Task<IEnumerable<Teacher>> GetTeachersAsync(CancellationToken cancellationToken = default)
            => apiClient.GetTeachersAsync(cancellationToken);

        public Task<IEnumerable<TimegridUnits>> GetTimegridsAsync(CancellationToken cancellationToken = default)
            => apiClient.GetTimegridsAsync(cancellationToken);

        public Task<IEnumerable<Timetable>> GetTimetablesAsync(TimetableParameters parameters, CancellationToken cancellationToken = default)
            => apiClient.GetTimetablesAsync(parameters, cancellationToken);

        public async Task<IEnumerable<Timetable>> GetTimetablesAsync(ComprehensiveTimetableParameters parameters, CancellationToken cancellationToken = default)
        {
            try
            {
                return await apiClient.GetTimetablesAsync(parameters, cancellationToken);
            }
            catch (JsonRpcException exception) when (exception.ErrorCode == -7002) // WebUntis: no such element.
            {
                var element = parameters.Options.Element;
                throw element.KeyType switch
                {
                    KeyTypes.Id => new NotFoundException($"No {element.Type} with {nameof(Element.Id)} {element.Id} found.", exception),
                    KeyTypes.Name => new NotFoundException($"No {element.Type} with {nameof(Element.Name)} \"{element.Id}\" found.", exception),
                    KeyTypes.ExternalKey => new NotFoundException($"No {element.Type} with {nameof(Element.ExternalKey)} \"{element.Id}\" found.", exception),
                    _ => exception
                };
            }
        }

        public async Task LogInAsync(string userName, string password, CancellationToken cancellationToken = default)
        {
            if (IsAuthenticated)
            {
                return;
            }

            try
            {
                await apiClient.LogInAsync(userName, password, cancellationToken);
            }
            catch (JsonRpcException exception) when (exception.ErrorCode == -8500) // WebUntis: invalid schoolname.
            {
                throw new NotFoundException($"No school or institute with the name \"{schoolOrInstituteName}\" found.");
            }
        }

        public Task LogOutAsync(CancellationToken cancellationToken = default)
            => this.LogOutAsync(force: false, cancellationToken);

        public async Task LogOutAsync(bool force, CancellationToken cancellationToken = default)
        {
            if (force)
            {
                await apiClient.LogOutAsync(cancellationToken);
            }
        }
    }
}
