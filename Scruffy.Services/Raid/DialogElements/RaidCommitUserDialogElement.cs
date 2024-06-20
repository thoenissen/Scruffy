using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// User input
/// </summary>
public class RaidCommitUserDialogElement : DialogMessageElementBase<IUser>
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

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetText("Message", "Please input the user.");

    /// <inheritdoc/>
    public override IUser ConvertMessage(IUserMessage message)
    {
        return CommandContext.Client.GetUser(MentionUtils.ParseUser(message.Content));
    }

    #endregion // DialogMessageElementBase
}