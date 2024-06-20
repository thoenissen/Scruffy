﻿using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Acquisition of the api kez
/// </summary>
public class GuildApiKeyDialogElement : DialogMessageElementBase<string>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildApiKeyDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogMessageElementBase<string>

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the api key.");

    /// <inheritdoc/>
    public override string ConvertMessage(IUserMessage message)
    {
        DialogContext.SetValue("ApiKey", message.Content);

        return base.ConvertMessage(message);
    }

    #endregion // DialogMessageElementBase<string>
}