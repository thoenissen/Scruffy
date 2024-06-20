﻿using System.Text.RegularExpressions;

using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Calendar.DialogElements;

/// <summary>
/// Removing participants
/// </summary>
public class CalendarRemoveParticipantsDialogElement : DialogMessageElementBase<List<IGuildUser>>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public CalendarRemoveParticipantsDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogMessageElementBase

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the members:");

    /// <inheritdoc/>
    public override List<IGuildUser> ConvertMessage(IUserMessage message)
    {
        var members = new List<IGuildUser>();

        foreach (Match match in new Regex("<@\\!?(\\d+?)>").Matches(message.Content))
        {
            members.Add(CommandContext.Guild.GetUserAsync(Convert.ToUInt64(match.Groups[1].Value)).Result);
        }

        return members;
    }

    #endregion // DialogMessageElementBase
}