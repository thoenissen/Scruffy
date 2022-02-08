using Discord;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Account.DialogElements;

/// <summary>
/// Acquisition of the api key
/// </summary>
public class AccountApiKeyDialogElement : DialogEmbedMessageElementBase<string>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public AccountApiKeyDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogMessageElementBase<string>

    /// <summary>
    /// Return the message of element
    /// </summary>
    /// <returns>Message</returns>
    public override EmbedBuilder GetMessage()
    {
        var builder = new EmbedBuilder();
        builder.WithTitle(LocalizationGroup.GetText("ChooseTitle", "Account Setup"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseDescription", "Please enter the api key which should be used. An API-Key can be created on the official [Guild Wars 2 Website](https://account.arena.net/applications.)."));

        return builder;
    }

        #endregion // DialogMessageElementBase<string>
    }