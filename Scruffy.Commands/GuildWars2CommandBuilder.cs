using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Scruffy.Services.Core;
using Scruffy.Services.GuildWars2;

namespace Scruffy.Commands
{
    /// <summary>
    /// Calendar commands
    /// </summary>
    [Group("gw2")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class GuildWars2CommandBuilder : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public GuildWars2CommandBuilder(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Quaggan service
        /// </summary>
        public QuagganService QuagganService { get; set; }

        #endregion // Properties

        #region Command methods

        /// <summary>
        /// Creation of a one time reminder
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("quaggan")]
        public Task RemindMeIn(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await QuagganService.PostRandomQuaggan(commandContextContainer)
                                                       .ConfigureAwait(false);
                               });
        }

        #endregion // Command methods
    }
}
