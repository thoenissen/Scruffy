using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using DSharpPlus;
using DSharpPlus.Entities;

using Newtonsoft.Json;

namespace Scruffy.Services.Core
{
    /// <summary>
    /// Providing emoji
    /// </summary>
    public static class DiscordEmojiService
    {
        #region Fields

        /// <summary>
        /// Emojis
        /// </summary>
        private static readonly ConcurrentDictionary<string, ulong> _emojis;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        static DiscordEmojiService()
        {
            _emojis = new ConcurrentDictionary<string, ulong>(JsonConvert.DeserializeObject<Dictionary<string, ulong>>(new StreamReader(Assembly.Load("Scruffy.Data").GetManifestResourceStream("Scruffy.Data.Resources.Emojis.json")).ReadToEnd()));
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Get 'Add'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetAddEmoji(DiscordClient discordClient) => GetEmoji(discordClient, "Add");

        /// <summary>
        /// Get 'Add2'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetAdd2Emoji(DiscordClient discordClient) => GetEmoji(discordClient, "Add2");

        /// <summary>
        /// Get 'Check'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetCheckEmoji(DiscordClient discordClient) => GetEmoji(discordClient, "Check");

        /// <summary>
        /// Get 'Cross'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetCrossEmoji(DiscordClient discordClient) => GetEmoji(discordClient, "Cross");

        /// <summary>
        /// Get 'Edit'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetEditEmoji(DiscordClient discordClient) => GetEmoji(discordClient, "Edit");

        /// <summary>
        /// Get 'Edit2'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetEdit2Emoji(DiscordClient discordClient) => GetEmoji(discordClient, "Edit2");

        /// <summary>
        /// Get 'Edit3'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetEdit3Emoji(DiscordClient discordClient) => GetEmoji(discordClient, "Edit3");

        /// <summary>
        /// Get 'Edit4'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetEdit4Emoji(DiscordClient discordClient) => GetEmoji(discordClient, "Edit4");

        /// <summary>
        /// Get 'Edit5'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetEdit5Emoji(DiscordClient discordClient) => GetEmoji(discordClient, "Edit5");

        /// <summary>
        /// Get 'TrashCan'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetTrashCanEmoji(DiscordClient discordClient) => GetEmoji(discordClient, "TrashCan");

        /// <summary>
        /// Get 'TrashCan2'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetTrashCan2Emoji(DiscordClient discordClient) => GetEmoji(discordClient, "TrashCan2");

        /// <summary>
        /// Get 'Emoji'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetEmojiEmoji(DiscordClient discordClient) => GetEmoji(discordClient, "Emoji");

        /// <summary>
        /// Get 'Empty'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetEmptyEmoji(DiscordClient discordClient) => GetEmoji(discordClient, "Empty");

        /// <summary>
        /// Get 'Bullet'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetBulletEmoji(DiscordClient discordClient) => GetEmoji(discordClient, "Bullet");

        /// <summary>
        /// Get 'Image'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetImageEmoji(DiscordClient discordClient) => GetEmoji(discordClient, "Image");

        /// <summary>
        /// Get 'Progress'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetProgressEmoji(DiscordClient discordClient) => GetEmoji(discordClient, "Progress");

        /// <summary>
        /// Get 'QuestionMark'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetQuestionMarkEmoji(DiscordClient discordClient) => GetEmoji(discordClient, "QuestionMark");

        /// <summary>
        /// Get 'GuildWars2Gold'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetGuildWars2GoldEmoji(DiscordClient discordClient) => GetEmoji(discordClient, "GuildWars2Gold");

        /// <summary>
        /// Get 'GuildWars2Silver'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetGuildWars2SilverEmoji(DiscordClient discordClient) => GetEmoji(discordClient, "GuildWars2Silver");

        /// <summary>
        /// Get 'GuildWars2Gold'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetGuildWars2CopperEmoji(DiscordClient discordClient) => GetEmoji(discordClient, "GuildWars2Copper");

        /// <summary>
        /// Get guild emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <param name="id">Id</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetGuildEmoji(DiscordClient discordClient, ulong id)
        {
            DiscordEmoji emoji;

            try
            {
                emoji = DiscordEmoji.FromGuildEmote(discordClient, id);
            }
            catch
            {
                emoji = DiscordEmoji.FromName(discordClient, ":grey_question:");
            }

            return emoji;
        }

        /// <summary>
        /// Get emoji by the given key
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <param name="key">key</param>
        /// <returns>Emoji</returns>
        private static DiscordEmoji GetEmoji(DiscordClient discordClient, string key)
        {
            try
            {
                return _emojis.TryGetValue(key, out var emojiId)
                           ? DiscordEmoji.FromGuildEmote(discordClient, emojiId)
                           : DiscordEmoji.FromName(discordClient, ":grey_question:");
            }
            catch
            {
                return DiscordEmoji.FromName(discordClient, ":grey_question:");
            }
        }

        #endregion // Methods
    }
}
