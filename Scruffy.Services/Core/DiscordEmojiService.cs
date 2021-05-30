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
        /// <summary>
        /// Emojis
        /// </summary>
        private static readonly ConcurrentDictionary<string, ulong> _emojis;

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
        /// Get 'TrashCan'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetTrashCanEmoji(DiscordClient discordClient) => GetEmoji(discordClient, "TrashCan");

        /// <summary>
        /// Get 'Emoji'-Emoji
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <returns>Emoji</returns>
        public static DiscordEmoji GetEmojiEmoji(DiscordClient discordClient) => GetEmoji(discordClient, "Emoji");

        /// <summary>
        /// Get emoji by the given key
        /// </summary>
        /// <param name="discordClient">Discord client</param>
        /// <param name="key">key</param>
        /// <returns>Emoji</returns>
        private static DiscordEmoji GetEmoji(DiscordClient discordClient, string key) => _emojis.TryGetValue(key, out var emojiId)
                                                                                             ? DiscordEmoji.FromGuildEmote(discordClient, emojiId)
                                                                                             : DiscordEmoji.FromName(discordClient, ":grey_question:");

        #endregion // Methods
    }
}
