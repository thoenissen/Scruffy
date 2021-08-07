using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Scruffy.Services.Account;
using Scruffy.Services.Core;

namespace Scruffy.Commands
{
    /// <summary>
    /// Calendar commands
    /// </summary>
    [Group("account")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class AccountCommandModule : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public AccountCommandModule(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Account administration service
        /// </summary>
        public AccountAdministrationService AdministrationService { get; set; }

        #endregion // Properties

        #region Command methods

        /// <summary>
        /// Adding a new account
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Command("add")]
        public async Task Add(CommandContext commandContext)
        {
            await AdministrationService.Add(commandContext)
                                       .ConfigureAwait(false);
        }

        /// <summary>
        /// Editing a account
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Command("edit")]
        public async Task Edit(CommandContext commandContext)
        {
            await AdministrationService.Edit(commandContext)
                                       .ConfigureAwait(false);
        }

        #endregion // Command methods

    }
}
