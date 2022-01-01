using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Infrastructure;
using HR.WebUntisConnector;
using HR.WebUntisConnector.Extensions;
using HR.WebUntisConnector.Model;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.Queries
{
    public class GetElementsByType : IQuery<IEnumerable<Element>>
    {
        public string InstituteName { get; set; }
        public ElementType ElementType { get; set; }
    }

    public class GetElementsByTypeHandler : IQueryHandler<GetElementsByType, IEnumerable<Element>>
    {
        private readonly IApiClientFactory apiClientFactory;

        public GetElementsByTypeHandler(ICachedApiClientFactory apiClientFactory)
            => this.apiClientFactory = Ensure.Argument.NotNull(() => apiClientFactory);

        public async Task<IEnumerable<Element>> HandleAsync(GetElementsByType query, CancellationToken cancellationToken)
        {
            var apiClient = await apiClientFactory.CreateApiClientAndLogInAsync(query.InstituteName, cancellationToken).ConfigureAwait(false);
            try
            {
                return query.ElementType switch
                {
                    ElementType.Klasse => await apiClient.GetKlassenAsync(cancellationToken).ConfigureAwait(false),
                    ElementType.Teacher => await apiClient.GetTeachersAsync(cancellationToken).ConfigureAwait(false),
                    ElementType.Subject => await apiClient.GetSubjectsAsync(cancellationToken).ConfigureAwait(false),
                    ElementType.Room => await apiClient.GetRoomsAsync(cancellationToken).ConfigureAwait(false),
                    ElementType.Student => await apiClient.GetStudentsAsync(cancellationToken).ConfigureAwait(false),
                    _ => throw new InvalidEnumArgumentException($"{nameof(query)}.{nameof(query.ElementType)}", Convert.ToInt32(query.ElementType), typeof(ElementType)),
                };
            }
            finally
            {
                await apiClient.LogOutAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
