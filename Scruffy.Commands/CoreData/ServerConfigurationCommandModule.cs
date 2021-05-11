using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Scruffy.Commands.Base;
using Scruffy.Services.Core;

namespace Scruffy.Commands.CoreData
{
    /// <summary>
    /// Configuration the server
    /// </summary>
    [Group("config")]
    [RequireUserPermissions(Permissions.Administrator)]
    public class ServerConfigurationCommandModule : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public ServerConfigurationCommandModule(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Prefix resolving
        /// </summary>
        public PrefixResolvingService PrefixResolvingService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Set the server prefix
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="prefix">Prefix</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("prefix")]
        public async Task SetPrefix(CommandContext commandContext, string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix) == false
             && prefix.Length > 0
             && prefix.Any(char.IsControl) == false)
            {
                PrefixResolvingService.AddOrRefresh(commandContext.Guild.Id, prefix);

                await commandContext.RespondAsync(LocalizationGroup.GetFormattedText("UsingNewPrefix", "I will use the following prefix: {0}", prefix))
                                    .ConfigureAwait(false);
            }
        }

        #endregion // Methods
    }
}
