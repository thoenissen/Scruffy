using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Account.DialogElements;

/// <summary>
/// Do you accept the extended data processing terns?
/// </summary>
public class AccountGuildStatisticsTermsDialogElement : DialogButtonElementBase<bool>
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
    public AccountGuildStatisticsTermsDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogButtonElementBase<bool>

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetFormattedText("AcceptTerms", "Do you agree to allow to use your data to use then in statistical visualisations?");

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