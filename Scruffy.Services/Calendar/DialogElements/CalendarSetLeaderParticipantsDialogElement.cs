﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using DSharpPlus.Entities;

using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;

namespace Scruffy.Services.Calendar.DialogElements
{
    /// <summary>
    /// Constructor
    /// </summary>
    public class CalendarSetLeaderParticipantsDialogElement : DialogMessageElementBase<List<DiscordMember>>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public CalendarSetLeaderParticipantsDialogElement(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region DialogMessageElementBase

        /// <summary>
        /// Return the message of element
        /// </summary>
        /// <returns>Message</returns>
        public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the members:");

        /// <summary>
        /// Converting the response message
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>Result</returns>
        public override List<DiscordMember> ConvertMessage(DiscordMessage message)
        {
            var members = new List<DiscordMember>();

            foreach (Match match in new Regex("<@\\!?(\\d+?)>").Matches(message.Content))
            {
                members.Add(CommandContext.Guild.GetMemberAsync(Convert.ToUInt64(match.Groups[1].Value)).Result);
            }

            return members;
        }

        #endregion // DialogMessageElementBase
    }
}