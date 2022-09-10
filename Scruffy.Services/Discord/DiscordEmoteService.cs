using System.Collections.Concurrent;
using System.IO;
using System.Reflection;

using Discord;
using Discord.Rest;
using Discord.WebSocket;

using Newtonsoft.Json;

namespace Scruffy.Services.Discord;

/// <summary>
/// Providing emoji
/// </summary>
public static class DiscordEmoteService
{
    #region Fields

    /// <summary>
    /// Emotes
    /// </summary>
    private static readonly ConcurrentDictionary<string, ulong> _emotes;

    /// <summary>
    /// Emote cache
    /// </summary>
    private static readonly ConcurrentDictionary<ulong, IEmote> _emoteCache;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    static DiscordEmoteService()
    {
        _emotes = new ConcurrentDictionary<string, ulong>(JsonConvert.DeserializeObject<Dictionary<string, ulong>>(new StreamReader(Assembly.Load("Scruffy.Data").GetManifestResourceStream("Scruffy.Data.Resources.Emotes.json")).ReadToEnd()));
        _emoteCache = new ConcurrentDictionary<ulong, IEmote>();
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Build emote cache
    /// </summary>
    /// <param name="client">Client</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task BuildEmoteCache(DiscordRestClient client)
    {
        foreach (var guild in await client.GetGuildsAsync()
                                          .ConfigureAwait(false))
        {
            foreach (var emote in guild.Emotes)
            {
                _emoteCache.TryAdd(emote.Id, emote);
            }
        }
    }

    /// <summary>
    /// Get 'Add'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetAddEmote(IDiscordClient client) => GetEmote(client, "Add");

    /// <summary>
    /// Get 'Add2'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetAdd2Emote(IDiscordClient client) => GetEmote(client, "Add2");

    /// <summary>
    /// Get 'Check'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetCheckEmote(IDiscordClient client) => GetEmote(client, "Check");

    /// <summary>
    /// Get 'Cross'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetCrossEmote(IDiscordClient client) => GetEmote(client, "Cross");

    /// <summary>
    /// Get 'Edit'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetEditEmote(IDiscordClient client) => GetEmote(client, "Edit");

    /// <summary>
    /// Get 'Edit2'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetEdit2Emote(IDiscordClient client) => GetEmote(client, "Edit2");

    /// <summary>
    /// Get 'Edit3'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetEdit3Emote(IDiscordClient client) => GetEmote(client, "Edit3");

    /// <summary>
    /// Get 'Edit4'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetEdit4Emote(IDiscordClient client) => GetEmote(client, "Edit4");

    /// <summary>
    /// Get 'Edit5'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetEdit5Emote(IDiscordClient client) => GetEmote(client, "Edit5");

    /// <summary>
    /// Get 'TrashCan'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetTrashCanEmote(IDiscordClient client) => GetEmote(client, "TrashCan");

    /// <summary>
    /// Get 'TrashCan2'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetTrashCan2Emote(IDiscordClient client) => GetEmote(client, "TrashCan2");

    /// <summary>
    /// Get 'Emote'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetEmojiEmote(IDiscordClient client) => GetEmote(client, "Emoji");

    /// <summary>
    /// Get 'Empty'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetEmptyEmote(IDiscordClient client) => GetEmote(client, "Empty");

    /// <summary>
    /// Get 'Bullet'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetBulletEmote(IDiscordClient client) => GetEmote(client, "Bullet");

    /// <summary>
    /// Get 'Image'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetImageEmote(IDiscordClient client) => GetEmote(client, "Image");

    /// <summary>
    /// Get 'QuestionMark'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetQuestionMarkEmote(IDiscordClient client) => GetEmote(client, "QuestionMark");

    /// <summary>
    /// Get 'GuildWars2Gold'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetGuildWars2GoldEmote(IDiscordClient client) => GetEmote(client, "GuildWars2Gold");

    /// <summary>
    /// Get 'GuildWars2Silver'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetGuildWars2SilverEmote(IDiscordClient client) => GetEmote(client, "GuildWars2Silver");

    /// <summary>
    /// Get 'GuildWars2Copper'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetGuildWars2CopperEmote(IDiscordClient client) => GetEmote(client, "GuildWars2Copper");

    /// <summary>
    /// Get 'ArrowUp'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetArrowUpEmote(IDiscordClient client) => GetEmote(client, "ArrowUp");

    /// <summary>
    /// Get 'ArrowDown'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetArrowDownEmote(IDiscordClient client) => GetEmote(client, "ArrowDown");

    /// <summary>
    /// Get 'Star'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetStarEmote(IDiscordClient client) => GetEmote(client, "Star");

    /// <summary>
    /// Get 'Loading'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetLoadingEmote(IDiscordClient client) => GetEmote(client, "Loading");

    /// <summary>
    /// Get 'First'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetFirstEmote(IDiscordClient client) => GetEmote(client, "First");

    /// <summary>
    /// Get 'Previous'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GePreviousEmote(IDiscordClient client) => GetEmote(client, "Previous");

    /// <summary>
    /// Get 'Next'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetNextEmote(IDiscordClient client) => GetEmote(client, "Next");

    /// <summary>
    /// Get 'Last'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetLastEmote(IDiscordClient client) => GetEmote(client, "Last");

    /// <summary>
    /// Get 'Guild Wars 2'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetGuildWars2Emote(IDiscordClient client) => GetEmote(client, "GuildWars2");

    /// <summary>
    /// Get 'GitHub'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetGitHubEmote(IDiscordClient client) => GetEmote(client, "GitHub");

    /// <summary>
    /// Get 'GW2 DPS Reports'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetDpsReportEmote(IDiscordClient client) => GetEmote(client, "DpsReport");

    /// <summary>
    /// Get 'Alacrity'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetAlacrityEmote(IDiscordClient client) => GetEmote(client, "Alacrity");

    /// <summary>
    /// Get 'Quickness'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetQuicknessEmote(IDiscordClient client) => GetEmote(client, "Quickness");

    /// <summary>
    /// Get 'Tank'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetTankEmote(IDiscordClient client) => GetEmote(client, "Tank");

    /// <summary>
    /// Get 'Healers'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetHealerEmote(IDiscordClient client) => GetEmote(client, "Healer");

    /// <summary>
    /// Get 'DamageDealer'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetDamageDealerEmote(IDiscordClient client) => GetEmote(client, "DamageDealer");

    /// <summary>
    /// Get guild emoji
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <param name="id">Id</param>
    /// <returns>Emote</returns>
    public static IEmote GetGuildEmote(IDiscordClient client, ulong id)
    {
        IEmote emote = null;

        try
        {
            if (client is BaseSocketClient socketClient)
            {
                emote = socketClient.Guilds
                                    .SelectMany(obj => obj.Emotes)
                                    .FirstOrDefault(obj => obj.Id == id);
            }
            else
            {
                _emoteCache.TryGetValue(id, out emote);
            }
        }
        catch
        {
        }

        return emote ?? Emoji.Parse(":grey_question:");
    }

    /// <summary>
    /// Get emoji by the given key
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <param name="key">key</param>
    /// <returns>Emote</returns>
    private static IEmote GetEmote(IDiscordClient client, string key)
    {
        IEmote emote = null;

        try
        {
            if (_emotes.TryGetValue(key, out var emojiId))
            {
                emote = GetGuildEmote(client, emojiId);
            }
        }
        catch
        {
        }

        return emote ?? Emoji.Parse(":grey_question:");
    }

    #endregion // Methods
}