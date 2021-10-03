using System;
using System.Globalization;

using DSharpPlus.Entities;

using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Raid.DialogElements
{
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

        /// <summary>
        /// Return the message of element
        /// </summary>
        /// <returns>Message</returns>
        public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the start time (hh:mm):");

        /// <summary>
        /// Converting the response message
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>Result</returns>
        public override TimeSpan ConvertMessage(DiscordMessage message)
        {
            return TimeSpan.TryParseExact(message.Content, "hh\\:mm", CultureInfo.InvariantCulture, out var timeSpan)
                       ? timeSpan
                       : throw new InvalidOperationException();
        }

        #endregion // DialogMessageElementBase<string>
    }
}