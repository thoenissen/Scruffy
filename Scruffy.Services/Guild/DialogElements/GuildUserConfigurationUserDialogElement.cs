using System.Text.RegularExpressions;

using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Adding participants
/// </summary>
public class GuildUserConfigurationUserDialogElement : DialogMessageElementBase<IGuildUser>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildUserConfigurationUserDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogMessageElementBase

    /// <summary>
    /// Return the message of element
    /// </summary>
    /// <returns>Message</returns>
    public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the member:");

    /// <summary>
    /// Converting the response message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>Result</returns>
    public override IGuildUser ConvertMessage(IUserMessage message)
    {
        var match = new Regex("<@\\!?(\\d+?)>").Matches(message.Content).FirstOrDefault();

        return match != null
                   ? CommandContext.Guild.GetUserAsync(Convert.ToUInt64(match.Groups[1].Value)).Result
                   : null;
    }

    #endregion // DialogMessageElementBase
}