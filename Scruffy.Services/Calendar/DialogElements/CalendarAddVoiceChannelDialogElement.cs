using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Calendar.DialogElements;

/// <summary>
/// Adding participants
/// </summary>
public class CalendarAddVoiceChannelDialogElement : DialogEmbedSelectMenuElementBase<List<IGuildUser>>
{
    #region Fields

    /// <summary>
    /// Channels
    /// </summary>
    private List<SelectMenuEntryData<List<IGuildUser>>> _channels;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public CalendarAddVoiceChannelDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogEmbedMessageElementBase

    /// <inheritdoc/>
    public override Task<EmbedBuilder> GetMessage()
    {
        return Task.FromResult(new EmbedBuilder().WithTitle(LocalizationGroup.GetText("ChooseTitle", "Voice channel selection"))
                                                 .WithDescription(LocalizationGroup.GetText("ChooseDescription", "Please choose one of the voice channels:"))
                                                 .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                                 .WithColor(Color.Green)
                                                 .WithTimestamp(DateTime.Now));
    }

    /// <inheritdoc/>
    protected override List<IGuildUser> DefaultFunc() => new();

    /// <inheritdoc/>
    public override IReadOnlyList<SelectMenuEntryData<List<IGuildUser>>> GetEntries()
    {
        if (_channels == null)
        {
            // TODO Async
            var channels = CommandContext.Guild
                                         .GetChannelsAsync()
                                         .Result;

            _channels = new List<SelectMenuEntryData<List<IGuildUser>>>();

            foreach (var channel in channels)
            {
                if (channel is IVoiceChannel voiceChannel)
                {
                    var users = voiceChannel.GetUsersAsync()
                                            .FlattenAsync()
                                            .Result;

                    if (users.Any(obj => obj.VoiceChannel?.Id == channel.Id))
                    {
                        _channels.Add(new SelectMenuEntryData<List<IGuildUser>>
                                      {
                                          CommandText = channel.Name,
                                          Response = async () => await GetUsers(channel.Id).ConfigureAwait(false)
                                      });
                    }
                }
            }
        }

        return _channels;
    }

    /// <summary>
    /// Get users of channel
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <returns>Result</returns>
    public async Task<List<IGuildUser>> GetUsers(ulong channelId)
    {
        var members = new List<IGuildUser>();

        if (await CommandContext.Guild
                                .GetChannelAsync(channelId)
                                .ConfigureAwait(false)
            is IVoiceChannel voiceChannel)
        {
            foreach (var user in await voiceChannel.GetUsersAsync()
                                                   .FlattenAsync()
                                                   .ConfigureAwait(false))
            {
                if (user.VoiceChannel?.Id == voiceChannel.Id)
                {
                    members.Add(user);
                }
            }
        }

        return members;
    }

    #endregion // DialogEmbedMessageElementBase
}