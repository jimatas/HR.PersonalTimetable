using Developist.Core.Cqrs.Queries;
using Developist.Core.Persistence;
using Developist.Core.Utilities;

using HR.PersonalCalendar.Model;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalCalendar.Queries
{
    public class GetPersonalTimetables : IQuery<IEnumerable<PersonalTimetable>>
    {
        public string UserName { get; set; }
    }

    public class GetPersonalTimetablesHandler : IQueryHandler<GetPersonalTimetables, IEnumerable<PersonalTimetable>>
    {
        private readonly IUnitOfWork uow;

        public GetPersonalTimetablesHandler(IUnitOfWork uow) 
            => this.uow = Ensure.Argument.NotNull(() => uow);

        public async Task<IEnumerable<PersonalTimetable>> HandleAsync(GetPersonalTimetables query, CancellationToken cancellationToken) 
            => await uow.Repository<PersonalTimetable>().FindAsync(table => table.UserName == query.UserName, cancellationToken).ConfigureAwait(false);
    }
}
