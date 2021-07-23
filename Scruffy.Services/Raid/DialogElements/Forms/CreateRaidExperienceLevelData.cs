using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Services.Raid.DialogElements.Forms
{
    /// <summary>
    /// Creation of a experience level
    /// </summary>
    public class CreateRaidExperienceLevelData
    {
        /// <summary>
        /// Superior role
        /// </summary>
        [DialogElementAssignment(typeof(RaidExperienceLevelSuperiorLevelDialogElement))]
        public long? SuperiorExperienceLevelId { get; set; }

        /// <summary>
        /// Discord role
        /// </summary>
        [DialogElementAssignment(typeof(RaidExperienceLevelRoleDialogElement))]
        public ulong? DiscordRoleId { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [DialogElementAssignment(typeof(RaidExperienceLevelDescriptionDialogElement))]
        public string Description { get; set; }

        /// <summary>
        /// AliasName
        /// </summary>
        [DialogElementAssignment(typeof(RaidExperienceLevelAliasNameDialogElement))]
        public string AliasName { get; set; }

        /// <summary>
        /// Discord emoji
        /// </summary>
        [DialogElementAssignment(typeof(RaidExperienceLevelEmojiDialogElement))]
        public ulong DiscordEmoji { get; set; }
    }
}
