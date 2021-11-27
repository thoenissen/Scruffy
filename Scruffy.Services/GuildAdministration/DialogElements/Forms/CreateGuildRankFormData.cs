using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Services.GuildAdministration.DialogElements.Forms
{
    /// <summary>
    /// Creation of a guild rank
    /// </summary>
    public class CreateGuildRankFormData
    {
        /// <summary>
        /// Superior rank
        /// </summary>
        [DialogElementAssignment(typeof(GuildAdministrationRankSuperiorRankDialogElement))]
        public int? SuperiorId { get; set; }

        /// <summary>
        /// Discord role id
        /// </summary>
        [DialogElementAssignment(typeof(GuildAdministrationRankDiscordRoleDialogElement))]
        public ulong DiscordRoleId { get; set; }

        /// <summary>
        /// In game name
        /// </summary>
        [DialogElementAssignment(typeof(GuildAdministrationRankInGameNameDialogElement))]
        public string InGameName { get; set; }

        /// <summary>
        /// Percentage quota
        /// </summary>
        [DialogElementAssignment(typeof(GuildAdministrationRankPercentageDialogElement))]
        public double Percentage { get; set; }
    }
}
