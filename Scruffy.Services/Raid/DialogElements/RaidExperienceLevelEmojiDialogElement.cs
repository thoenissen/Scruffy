using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// Acquisition of the experience level emoji
/// </summary>
public class RaidExperienceLevelEmojiDialogElement : DialogReactionElementBase<ulong>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public RaidExperienceLevelEmojiDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogReactionElementBase<bool>

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetText("EmojiPrompt", "Please react with emoji which should be used.");

    /// <inheritdoc/>
    protected override ulong DefaultFunc(IReaction reaction) => (reaction.Emote as ISnowflakeEntity)?.Id ?? throw new InvalidOperationException();

    #endregion // DialogReactionElementBase<bool>
}