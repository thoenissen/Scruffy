using DSharpPlus.CommandsNext;

using Scruffy.Services.Core;

namespace Scruffy.Commands
{
    /// <summary>
    /// Command module base class with localization services
    /// </summary>
    public class LocatedCommandModuleBase : BaseCommandModule
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public LocatedCommandModuleBase(LocalizationService localizationService)
        {
            LocalizationGroup = localizationService.GetGroup(GetType().Name);
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Localization group
        /// </summary>
        public LocalizationGroup LocalizationGroup { get; }

        #endregion // Properties
    }
}
