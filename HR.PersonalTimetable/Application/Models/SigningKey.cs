using HR.Common.Persistence.Entities;

using System;
using System.Text.Json.Serialization;

namespace HR.PersonalTimetable.Application.Models
{
    /// <summary>
    /// Encapsulates a signing key and its creation timestamp.
    /// </summary>
    public class SigningKey : EntityBase<Guid>
    {
        public SigningKey() { }
        public SigningKey(Guid id) => Id = id;

        [JsonIgnore]
        public override bool IsTransient => base.IsTransient;

        /// <summary>
        /// The actual signing key value.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The integration this signing key belongs to.
        /// </summary>
        [JsonIgnore]
        public Integration Integration { get; set; }

        public DateTimeOffset DateCreated { get; set; }

        /// <inheritdoc/>
        public override string ToString() => Key ?? string.Empty;
    }
}
