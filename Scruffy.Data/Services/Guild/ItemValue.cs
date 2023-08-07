namespace Scruffy.Data.Services.Guild
{
    /// <summary>
    /// Item value
    /// </summary>
    public class ItemValue
    {
        /// <summary>
        /// Is upgrade?
        /// </summary>
        public bool IsUpgrade { get; set; }

        /// <summary>
        /// Count
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        public long? Value { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}