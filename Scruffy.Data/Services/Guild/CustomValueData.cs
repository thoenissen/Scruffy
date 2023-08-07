namespace Scruffy.Data.Services.Guild
{
    /// <summary>
    /// Custom value data
    /// </summary>
    public class CustomValueData
    {
        /// <summary>
        /// Item id
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        public long Value { get; set; }

        /// <summary>
        /// Custom value threshold
        /// </summary>
        public bool IsCustomValueThresholdActivated { get; set; }
    }
}