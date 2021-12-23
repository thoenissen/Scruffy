using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Services.Guild.DialogElements.Forms
{
    /// <summary>
    /// Creation of a guild rank
    /// </summary>
    public class CreateGuildRankFormData
    {
        /// <summary>
        /// Superior rank
        /// </summary>
        [DialogElementAssignment(typeof(GuildRankSuperiorRankDialogElement))]
        public int? SuperiorId { get; set; }

        /// <summary>
        /// Discord role id
        /// </summary>
        [DialogElementAssignment(typeof(GuildRankDiscordRoleDialogElement))]
        public ulong DiscordRoleId { get; set; }

        /// <summary>
        /// In game name
        /// </summary>
        [DialogElementAssignment(typeof(GuildRankInGameNameDialogElement))]
        public string InGameName { get; set; }

        /// <summary>
        /// Percentage quota
        /// </summary>
        [DialogElementAssignment(typeof(GuildRankPercentageDialogElement))]
        public double Percentage { get; set; }
    }
}
