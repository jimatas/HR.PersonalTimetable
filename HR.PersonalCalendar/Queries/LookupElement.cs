using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Infrastructure;
using HR.WebUntisConnector;
using HR.WebUntisConnector.Extensions;
using HR.WebUntisConnector.Model;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.Queries
{
    public class LookupElement : IQuery<Element>
    {
        public string InstituteName { get; set; }
        public string ElementName { get; set; }
        public ElementType ElementType { get; set; }
    }

    public class LookupElementHandler : IQueryHandler<LookupElement, Element>
    {
        private readonly IApiClientFactory apiClientFactory;

        public LookupElementHandler(ICachedApiClientFactory apiClientFactory)
            => this.apiClientFactory = Ensure.Argument.NotNull(() => apiClientFactory);

        public async Task<Element> HandleAsync(LookupElement query, CancellationToken cancellationToken)
        {
            var apiClient = await apiClientFactory.CreateApiClientAndLogInAsync(query.InstituteName, cancellationToken).ConfigureAwait(false);
            try
            {
                const StringComparison ignoringCase = StringComparison.InvariantCultureIgnoreCase;
                return query.ElementType switch
                {
                    ElementType.Klasse => (await apiClient.GetKlassenAsync(cancellationToken).ConfigureAwait(false)).FirstOrDefault(k => k.Name.Equals(query.ElementName, ignoringCase)),
                    ElementType.Teacher => (await apiClient.GetTeachersAsync(cancellationToken).ConfigureAwait(false)).FirstOrDefault(t => t.Name.Equals(query.ElementName, ignoringCase)),
                    ElementType.Subject => (await apiClient.GetSubjectsAsync(cancellationToken).ConfigureAwait(false)).FirstOrDefault(s => s.Name.Equals(query.ElementName, ignoringCase)),
                    ElementType.Room => (await apiClient.GetRoomsAsync(cancellationToken).ConfigureAwait(false)).FirstOrDefault(r => r.Name.Equals(query.ElementName, ignoringCase)),
                    _ => throw new ArgumentOutOfRangeException(nameof(query), $"{nameof(query)}.{nameof(query.ElementType)} is not a valid {nameof(ElementType)}."),
                };
            }
            finally
            {
                await apiClient.LogOutAsync(cancellationToken);
            }
        }
    }
}
