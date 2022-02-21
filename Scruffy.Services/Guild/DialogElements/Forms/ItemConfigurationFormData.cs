using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Services.Guild.DialogElements.Forms
{
    /// <summary>
    /// Item configuration
    /// </summary>
    public class ItemConfigurationFormData
    {
        /// <summary>
        /// Custom value
        /// </summary>
        [DialogElementAssignment(typeof(GuildConfigurationItemCustomerValueDialogElement))]
        public long CustomValue { get; set; }

        /// <summary>
        /// Should the value be reduced after n inserts.
        /// </summary>
        [DialogElementAssignment(typeof(GuildConfigurationItemCustomValueThresholdDialogElement))]
        public int? CustomValueThreshold { get; set; }
    }
}
