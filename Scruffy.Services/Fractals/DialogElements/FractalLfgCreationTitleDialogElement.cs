using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Fractals.DialogElements
{
    /// <summary>
    /// Acquisition of the title
    /// </summary>
    public class FractalLfgCreationTitleDialogElement : DialogMessageElementBase<string>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public FractalLfgCreationTitleDialogElement(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region DialogMessageElementBase<string>

        /// <summary>
        /// Return the message of element
        /// </summary>
        /// <returns>Message</returns>
        public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the title which should be used.");

        #endregion // DialogMessageElementBase<string>
    }
}