using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Services.GuildAdministration.DialogElements.Forms
{
    /// <summary>
    /// Creation of a guild special rank
    /// </summary>
    public class CreateGuildSpecialRankData
    {
        /// <summary>
        /// Description
        /// </summary>
        [DialogElementAssignment(typeof(GuildAdministrationSpecialRankDescriptionDialogElement))]
        public string Description { get; set; }

        /// <summary>
        /// Id of the discord role
        /// </summary>
        [DialogElementAssignment(typeof(GuildAdministrationSpecialRankDiscordRoleDialogElement))]
        public ulong DiscordRoleId { get; set; }

        /// <summary>
        /// Maximum points
        /// </summary>
        [DialogElementAssignment(typeof(GuildAdministrationSpecialRankMaximumPointsDialogElement))]
        public double MaximumPoints { get; set; }

        /// <summary>
        /// Grand role threshold
        /// </summary>
        [DialogElementAssignment(typeof(GuildAdministrationSpecialRankGrantThresholdDialogElement))]
        public double GrantThreshold { get; set; }

        /// <summary>
        /// Remove role threshold
        /// </summary>
        [DialogElementAssignment(typeof(GuildAdministrationSpecialRankRemoveThresholdDialogElement))]
        public double RemoveThreshold { get; set; }
    }
}
