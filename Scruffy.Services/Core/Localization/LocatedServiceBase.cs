namespace Scruffy.Services.Core.Localization
{
    /// <summary>
    /// Command module base class with localization services
    /// </summary>
    public class LocatedServiceBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public LocatedServiceBase(LocalizationService localizationService)
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