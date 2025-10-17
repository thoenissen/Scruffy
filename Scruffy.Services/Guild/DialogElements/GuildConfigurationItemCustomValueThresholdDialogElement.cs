using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Acquisition of the custom value threshold
/// </summary>
public class GuildConfigurationItemCustomValueThresholdDialogElement : DialogButtonElementBase<bool>
{
    #region Fields

    /// <summary>
    /// Buttons
    /// </summary>
    private List<ButtonData<bool>> _buttons;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildConfigurationItemCustomValueThresholdDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogReactionElementBase<bool>

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetText("Message", "Should the threshold be considered for this item?");

    /// <inheritdoc/>
    public override IReadOnlyList<ButtonData<bool>> GetButtons()
    {
        return _buttons ??= [
                                new ButtonData<bool>
                                {
                                    CommandText = LocalizationGroup.GetText("Yes", "Yes"),
                                    Emote = DiscordEmoteService.GetCheckEmote(CommandContext.Client),
                                    Func = () => Task.FromResult(true)
                                },
                                new ButtonData<bool>
                                {
                                    CommandText = LocalizationGroup.GetText("No", "No"),
                                    Emote = DiscordEmoteService.GetCrossEmote(CommandContext.Client),
                                    Func = () => Task.FromResult(false)
                                },
                            ];
    }

    /// <inheritdoc/>
    protected override bool DefaultFunc() => false;

    #endregion // DialogReactionElementBase<bool>
}