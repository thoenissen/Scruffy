using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Account.DialogElements;

/// <summary>
/// Do you accept the data processing terms?
/// </summary>
public class AccountDataProcessingTermsDialogElement : DialogButtonElementBase<bool>
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
    public AccountDataProcessingTermsDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogButtonElementBase<bool>

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetText("AcceptStorage", "To use this feature, you must agree to the processing and storage of your data to provide the functionalities of this bot. The command `/info` can be used to display a detailed list of the data that will be stored and processed. Do you agree to this processing and storage?");

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
                                }
                            ];
    }

    /// <inheritdoc/>
    protected override bool DefaultFunc() => false;

    #endregion // DialogButtonElementBase<bool>
}