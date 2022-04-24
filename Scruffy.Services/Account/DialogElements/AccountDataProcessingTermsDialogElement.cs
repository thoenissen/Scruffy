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

    /// <summary>
    /// Editing the message
    /// </summary>
    /// <returns>Message</returns>
    public override string GetMessage() => LocalizationGroup.GetText("AcceptStorage", "To use this feature, you must agree to the processing and storage of your data to provide the functionalities of this bot. The command `/info` can be used to display a detailed list of the data that will be stored and processed. Do you agree to this processing and storage?");

    /// <summary>
    /// Returns the buttons which should be added to the message
    /// </summary>
    /// <returns>Reactions</returns>
    public override IReadOnlyList<ButtonData<bool>> GetButtons()
    {
        return _buttons ??= new List<ButtonData<bool>>
                            {
                                new()
                                {
                                    CommandText = LocalizationGroup.GetText("Yes", "Yes"),
                                    Emote = DiscordEmoteService.GetCheckEmote(CommandContext.Client),
                                    Func = () => Task.FromResult(true)
                                },
                                new()
                                {
                                    CommandText = LocalizationGroup.GetText("No", "No"),
                                    Emote = DiscordEmoteService.GetCrossEmote(CommandContext.Client),
                                    Func = () => Task.FromResult(false)
                                },
                            };
    }

    /// <summary>
    /// Default case if none of the given reactions is used
    /// </summary>
    /// <returns>Result</returns>
    protected override bool DefaultFunc() => false;

    #endregion // DialogButtonElementBase<bool>
}