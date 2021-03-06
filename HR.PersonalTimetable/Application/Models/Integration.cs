using HR.Common.Persistence.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace HR.PersonalTimetable.Application.Models
{
    /// <summary>
    /// Identifies a particular API integration.
    /// </summary>
    public class Integration : EntityBase<Guid>
    {
        public Integration() { }
        public Integration(Guid id) => Id = id;

        [JsonIgnore]
        public override bool IsTransient => base.IsTransient;

        /// <summary>
        /// The unique name of the integation.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The current and possibly previous signing keys of this particular integration.
        /// </summary>
        public ICollection<SigningKey> SigningKeys { get; set; }

        public SigningKey CurrentSigningKey => SigningKeys?.OrderBy(key => key.DateCreated).LastOrDefault();
    }
}
