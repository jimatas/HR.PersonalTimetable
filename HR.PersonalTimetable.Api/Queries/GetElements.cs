using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Infrastructure;
using HR.WebUntisConnector;
using HR.WebUntisConnector.Extensions;
using HR.WebUntisConnector.Model;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Api.Queries
{
    public class GetElements : IQuery<IEnumerable<Element>>
    {
        [Required]
        [FromQuery(Name = "institute")]
        public string InstituteName { get; set; }

        [Required]
        [FromQuery(Name = "type")]
        public ElementType ElementType { get; set; }
    }

    public class GetElementsHandler : IQueryHandler<GetElements, IEnumerable<Element>>
    {
        private readonly IApiClientFactory apiClientFactory;

        public GetElementsHandler(ICachedApiClientFactory apiClientFactory)
        {
            this.apiClientFactory = Ensure.Argument.NotNull(() => apiClientFactory);
        }

        public async Task<IEnumerable<Element>> HandleAsync(GetElements query, CancellationToken cancellationToken)
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
