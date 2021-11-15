using System.Threading.Tasks;

using Scruffy.Services.Calendar.DialogElements;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Calendar;

/// <summary>
/// Calendar template service
/// </summary>
public class CalendarTemplateService : LocatedServiceBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public CalendarTemplateService(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Managing the templates
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RunAssistantAsync(CommandContextContainer commandContext)
    {
        bool repeat;

        do
        {
            repeat = await DialogHandler.Run<CalendarTemplateSetupDialogElement, bool>(commandContext).ConfigureAwait(false);
        }
        while (repeat);
    }

    #endregion // Methods
}