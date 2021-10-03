using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Account.DialogElements
{
    /// <summary>
    /// Acquisition of the dps report user token
    /// </summary>
    public class AccountDpsReportUserTokenDialogElement : DialogMessageElementBase<string>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public AccountDpsReportUserTokenDialogElement(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region DialogMessageElementBase<string>

        /// <summary>
        /// Return the message of element
        /// </summary>
        /// <returns>Message</returns>
        public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the dps.report user token which should be used.");

        #endregion // DialogMessageElementBase<string>
    }
}