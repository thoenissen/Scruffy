﻿using System.Globalization;

using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// Acquisition of the start time of the raid day configuration
/// </summary>
public class RaidDayStartTimeDialogElement : DialogMessageElementBase<TimeSpan>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public RaidDayStartTimeDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogMessageElementBase<string>

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the start time (hh:mm):");

    /// <inheritdoc/>
    public override TimeSpan ConvertMessage(IUserMessage message)
    {
        return TimeSpan.TryParseExact(message.Content, "hh\\:mm", CultureInfo.InvariantCulture, out var timeSpan)
                   ? timeSpan
                   : throw new InvalidOperationException();
    }

    #endregion // DialogMessageElementBase<string>
}