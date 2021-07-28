using System.Threading.Tasks;

using DSharpPlus.CommandsNext;

using Scruffy.Services.Calendar.DialogElements;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;

namespace Scruffy.Services.Calendar
{
    /// <summary>
    /// Calendar schedule service
    /// </summary>
    public class CalendarScheduleService : LocatedServiceBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public CalendarScheduleService(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Managing the Schedules
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RunAssistantAsync(CommandContext commandContext)
        {
            bool repeat;

            do
            {
                repeat = await DialogHandler.Run<CalendarScheduleSetupDialogElement, bool>(commandContext).ConfigureAwait(false);
            }
            while (repeat);
        }

        #endregion // Methods
    }
}