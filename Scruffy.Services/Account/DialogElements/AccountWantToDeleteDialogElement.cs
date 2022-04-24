using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Account.DialogElements;

/// <summary>
/// Do you want to add a new account?
/// </summary>
public class AccountWantToDeleteDialogElement : DialogButtonElementBase<bool>
{
    #region Fields

    /// <summary>
    /// Account name
    /// </summary>
    private readonly string _name;

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
    /// <param name="name">Account name</param>
    public AccountWantToDeleteDialogElement(LocalizationService localizationService, string name)
        : base(localizationService)
    {
        _name = name;
    }

    #endregion // Constructor

    #region DialogButtonElementBase<bool>

    /// <summary>
    /// Editing the message
    /// </summary>
    /// <returns>Message</returns>
    public override string GetMessage() => LocalizationGroup.GetFormattedText("Prompt", "Are you sure you want to delete the account '{0}'?", _name);

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