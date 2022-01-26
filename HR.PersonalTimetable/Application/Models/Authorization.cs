namespace HR.PersonalTimetable.Application.Models
{
    public class Authorization
    {
        /// <summary>
        /// The client-provided hash that is to be verified.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The client's signing key.
        /// </summary>
        public SigningKey SigningKey { get; set; }

        /// <summary>
        /// The client-provided Unix timestamp in seconds
        /// </summary>
        public long Timestamp { get; set; }
    }
}
