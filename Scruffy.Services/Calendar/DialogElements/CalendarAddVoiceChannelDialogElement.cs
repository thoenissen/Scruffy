using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Calendar.DialogElements;

/// <summary>
/// Adding participants
/// </summary>
public class CalendarAddVoiceChannelDialogElement : DialogEmbedMessageElementBase<List<IGuildUser>>
{
    #region Fields

    /// <summary>
    /// Appointments
    /// </summary>
    private Dictionary<int, ulong> _channels;

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

    /// <summary>
    /// Return the message of element
    /// </summary>
    /// <returns>Message</returns>
    public override EmbedBuilder GetMessage()
    {
        var builder = new EmbedBuilder();
        builder.WithTitle(LocalizationGroup.GetText("ChooseTitle", "Voice channel selection"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseDescription", "Please choose one of the voice channels:"));

        _channels = new Dictionary<int, ulong>();
        var fieldText = new StringBuilder();

        var i = 0;

        // TODO Async
        foreach (var channel in CommandContext.Guild.GetChannelsAsync().Result.OfType<IVoiceChannel>())
        {
            fieldText.Append('`');
            fieldText.Append(i);
            fieldText.Append("` - ");
            fieldText.Append(' ');
            fieldText.Append(channel.Mention);
            fieldText.Append('\n');

            _channels[i] = channel.Id;

            i++;
        }

        builder.AddField(LocalizationGroup.GetText("Field", "Channels"), fieldText.ToString());

        return builder;
    }

    /// <summary>
    /// Converting the response message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>Result</returns>
    public override async Task<List<IGuildUser>> ConvertMessage(IUserMessage message)
    {
        var members = new List<IGuildUser>();

        if (int.TryParse(message.Content, out var index)
         && _channels.TryGetValue(index, out var selected))
        {
            if (await CommandContext.Guild.GetChannelAsync(selected).ConfigureAwait(false) is IVoiceChannel voiceChannel)
            {
                await foreach (var entry in voiceChannel.GetUsersAsync())
                {
                    foreach (var user in entry.Where(obj => obj.VoiceChannel?.Id == voiceChannel.Id))
                    {
                        members.Add(user);
                    }
                }
            }
        }

        return members;
    }

    #endregion // DialogEmbedMessageElementBase
}