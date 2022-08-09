using System.Collections.Concurrent;
using System.IO;
using System.Reflection;

using Discord;
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

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    static DiscordEmoteService()
    {
        _emotes = new ConcurrentDictionary<string, ulong>(JsonConvert.DeserializeObject<Dictionary<string, ulong>>(new StreamReader(Assembly.Load("Scruffy.Data").GetManifestResourceStream("Scruffy.Data.Resources.Emotes.json")).ReadToEnd()));
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Get 'Add'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetAddEmote(BaseSocketClient client) => GetEmote(client, "Add");

    /// <summary>
    /// Get 'Add2'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetAdd2Emote(BaseSocketClient client) => GetEmote(client, "Add2");

    /// <summary>
    /// Get 'Check'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetCheckEmote(BaseSocketClient client) => GetEmote(client, "Check");

    /// <summary>
    /// Get 'Cross'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetCrossEmote(BaseSocketClient client) => GetEmote(client, "Cross");

    /// <summary>
    /// Get 'Edit'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetEditEmote(BaseSocketClient client) => GetEmote(client, "Edit");

    /// <summary>
    /// Get 'Edit2'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetEdit2Emote(BaseSocketClient client) => GetEmote(client, "Edit2");

    /// <summary>
    /// Get 'Edit3'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetEdit3Emote(BaseSocketClient client) => GetEmote(client, "Edit3");

    /// <summary>
    /// Get 'Edit4'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetEdit4Emote(BaseSocketClient client) => GetEmote(client, "Edit4");

    /// <summary>
    /// Get 'Edit5'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetEdit5Emote(BaseSocketClient client) => GetEmote(client, "Edit5");

    /// <summary>
    /// Get 'TrashCan'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetTrashCanEmote(BaseSocketClient client) => GetEmote(client, "TrashCan");

    /// <summary>
    /// Get 'TrashCan2'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetTrashCan2Emote(BaseSocketClient client) => GetEmote(client, "TrashCan2");

    /// <summary>
    /// Get 'Emote'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetEmojiEmote(BaseSocketClient client) => GetEmote(client, "Emoji");

    /// <summary>
    /// Get 'Empty'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetEmptyEmote(BaseSocketClient client) => GetEmote(client, "Empty");

    /// <summary>
    /// Get 'Bullet'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetBulletEmote(BaseSocketClient client) => GetEmote(client, "Bullet");

    /// <summary>
    /// Get 'Image'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetImageEmote(BaseSocketClient client) => GetEmote(client, "Image");

    /// <summary>
    /// Get 'QuestionMark'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetQuestionMarkEmote(BaseSocketClient client) => GetEmote(client, "QuestionMark");

    /// <summary>
    /// Get 'GuildWars2Gold'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetGuildWars2GoldEmote(BaseSocketClient client) => GetEmote(client, "GuildWars2Gold");

    /// <summary>
    /// Get 'GuildWars2Silver'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetGuildWars2SilverEmote(BaseSocketClient client) => GetEmote(client, "GuildWars2Silver");

    /// <summary>
    /// Get 'GuildWars2Copper'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetGuildWars2CopperEmote(BaseSocketClient client) => GetEmote(client, "GuildWars2Copper");

    /// <summary>
    /// Get 'ArrowUp'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetArrowUpEmote(BaseSocketClient client) => GetEmote(client, "ArrowUp");

    /// <summary>
    /// Get 'ArrowDown'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetArrowDownEmote(BaseSocketClient client) => GetEmote(client, "ArrowDown");

    /// <summary>
    /// Get 'Star'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetStarEmote(BaseSocketClient client) => GetEmote(client, "Star");

    /// <summary>
    /// Get 'Loading'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetLoadingEmote(BaseSocketClient client) => GetEmote(client, "Loading");

    /// <summary>
    /// Get 'First'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetFirstEmote(BaseSocketClient client) => GetEmote(client, "First");

    /// <summary>
    /// Get 'Previous'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GePreviousEmote(BaseSocketClient client) => GetEmote(client, "Previous");

    /// <summary>
    /// Get 'Next'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetNextEmote(BaseSocketClient client) => GetEmote(client, "Next");

    /// <summary>
    /// Get 'Last'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetLastEmote(BaseSocketClient client) => GetEmote(client, "Last");

    /// <summary>
    /// Get 'Guild Wars 2'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetGuildWars2Emote(BaseSocketClient client) => GetEmote(client, "GuildWars2");

    /// <summary>
    /// Get 'GitHub'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetGitHubEmote(BaseSocketClient client) => GetEmote(client, "GitHub");

    /// <summary>
    /// Get 'GW2 DPS Reports'-Emote
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <returns>Emote</returns>
    public static IEmote GetDpsReportEmote(BaseSocketClient client) => GetEmote(client, "DpsReport");

    /// <summary>
    /// Get guild emoji
    /// </summary>
    /// <param name="client">Discord client</param>
    /// <param name="id">Id</param>
    /// <returns>Emote</returns>
    public static IEmote GetGuildEmote(BaseSocketClient client, ulong id)
    {
        IEmote emote = null;

        try
        {
            emote = client.Guilds
                          .SelectMany(obj => obj.Emotes)
                          .FirstOrDefault(obj => obj.Id == id);
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
    private static IEmote GetEmote(BaseSocketClient client, string key)
    {
        IEmote emote = null;

        try
        {
            if (_emotes.TryGetValue(key, out var emojiId))
            {
                emote = client.Guilds.SelectMany(obj => obj.Emotes).FirstOrDefault(obj => obj.Id == emojiId);
            }
        }
        catch
        {
        }

        return emote ?? Emoji.Parse(":grey_question:");
    }

    #endregion // Methods
}