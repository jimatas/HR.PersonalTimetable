using Developist.Core.Cqrs.Queries;
using Developist.Core.Persistence;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Application.Validators;

using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Application.Queries
{
    public class GetPersonalTimetables : IQuery<IEnumerable<Application.Models.PersonalTimetable>>
    {
        /// <summary>
        /// The username of the user. 
        /// Depending on their role, either an employee code or a student number.
        /// </summary>
        [Required, UserName]
        [FromRoute(Name = "user")]
        public string UserName { get; set; }
    }

    public class GetPersonalTimetablesHandler : IQueryHandler<GetPersonalTimetables, IEnumerable<Application.Models.PersonalTimetable>>
    {
        private readonly IUnitOfWork unitOfWork;

        public GetPersonalTimetablesHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = Ensure.Argument.NotNull(() => unitOfWork);
        }

        public async Task<IEnumerable<Application.Models.PersonalTimetable>> HandleAsync(GetPersonalTimetables query, CancellationToken cancellationToken)
        {
            return await unitOfWork.Repository<Application.Models.PersonalTimetable>().FindAsync(table => table.UserName == query.UserName, cancellationToken).WithoutCapturingContext();
        }
    }
}
