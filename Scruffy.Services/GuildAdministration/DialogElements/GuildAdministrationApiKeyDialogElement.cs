using DSharpPlus.Entities;

using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.GuildAdministration.DialogElements
{
    /// <summary>
    /// Acquisition of the api kez
    /// </summary>
    public class GuildAdministrationApiKeyDialogElement : DialogMessageElementBase<string>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public GuildAdministrationApiKeyDialogElement(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region DialogMessageElementBase<string>

        /// <summary>
        /// Return the message of element
        /// </summary>
        /// <returns>Message</returns>
        public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the api key.");

        /// <summary>
        /// Converting the response message
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>Result</returns>
        public override string ConvertMessage(DiscordMessage message)
        {
            DialogContext.SetValue("ApiKey", message.Content);

            return base.ConvertMessage(message);
        }

        #endregion // DialogMessageElementBase<string>
    }
}