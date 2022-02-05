using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// Removing a user from the commit
/// </summary>
public class RaidCommitRemoveUserDialogElement : DialogMessageElementBase<IUser>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public RaidCommitRemoveUserDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogMessageElementBase

    /// <summary>
    /// Return the message of element
    /// </summary>
    /// <returns>Message</returns>
    public override string GetMessage() => LocalizationGroup.GetText("Message", "Which user should be removed?");

    /// <summary>
    /// Converting the response message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>Result</returns>
    public override IUser ConvertMessage(IUserMessage message)
    {
        return CommandContext.Client.GetUser(MentionUtils.ParseUser(message.Content));
    }

    #endregion // DialogMessageElementBase
}