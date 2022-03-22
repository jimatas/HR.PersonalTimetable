using HR.Common.Cqrs.Queries;
using HR.Common.Persistence;
using HR.Common.Utilities;
using HR.PersonalTimetable.Application.Validators;

using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Application.Queries
{
    public class GetPersonalTimetables : IQuery<IEnumerable<Models.PersonalTimetable>>
    {
        /// <summary>
        /// The username of the user. 
        /// Depending on their role, either an employee code or a student number.
        /// </summary>
        [Required, UserName]
        [FromRoute(Name = "user")]
        public string UserName { get; set; }
    }

    public class GetPersonalTimetablesHandler : IQueryHandler<GetPersonalTimetables, IEnumerable<Models.PersonalTimetable>>
    {
        private readonly IUnitOfWork unitOfWork;

        public GetPersonalTimetablesHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = Ensure.Argument.NotNull(() => unitOfWork);
        }

        public async Task<IEnumerable<Models.PersonalTimetable>> HandleAsync(GetPersonalTimetables query, CancellationToken cancellationToken)
        {
            return await unitOfWork.Repository<Models.PersonalTimetable>().FindAsync(table => table.UserName == query.UserName, cancellationToken);
        }
    }
}
