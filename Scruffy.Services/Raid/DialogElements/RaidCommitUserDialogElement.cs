using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// User input
/// </summary>
public class RaidCommitUserDialogElement : DialogMessageElementBase<DiscordUser>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public RaidCommitUserDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogMessageElementBase

    /// <summary>
    /// Return the message of element
    /// </summary>
    /// <returns>Message</returns>
    public override string GetMessage() => LocalizationGroup.GetText("Message", "Please input the user.");

    /// <summary>
    /// Converting the response message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>Result</returns>
    public override DiscordUser ConvertMessage(DiscordMessage message)
    {
        var converter = (IArgumentConverter<DiscordUser>)new DiscordUserConverter();

        return converter.ConvertAsync(message.Content, CommandContext.GetCommandContext()).Result.Value;
    }

    #endregion // DialogMessageElementBase
}