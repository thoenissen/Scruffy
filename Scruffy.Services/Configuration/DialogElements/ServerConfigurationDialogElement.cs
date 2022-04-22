using System.Reflection;

using Discord;
using Discord.Interactions;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.CoreData;
using Scruffy.Services.Core;
using Scruffy.Services.Core.DialogElements;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Configuration.DialogElements
{
    /// <summary>
    /// Server configuration
    /// </summary>
    public class ServerConfigurationDialogElement : DialogEmbedSelectMenuElementBase<bool>
    {
        #region Fields

        /// <summary>
        /// Administration commands
        /// </summary>
        private static readonly List<string> _administrationCommands = new()
                                                                       {
                                                                           "admin",
                                                                           "games",
                                                                           "guild-admin",
                                                                           "raid-admin"
                                                                       };

        /// <summary>
        /// Repository factory
        /// </summary>
        private readonly RepositoryFactory _repositoryFactory;

        /// <summary>
        /// Permissions validation service
        /// </summary>
        private readonly AdministrationPermissionsValidationService _permissionsValidationService;

        /// <summary>
        /// Interaction service
        /// </summary>
        private readonly InteractionService _interactionService;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="repositoryFactory">Repository factory</param>
        /// <param name="permissionsValidationService">Permission validation service</param>
        /// <param name="interactionService">Interaction service</param>
        public ServerConfigurationDialogElement(LocalizationService localizationService,
                                                RepositoryFactory repositoryFactory,
                                                AdministrationPermissionsValidationService permissionsValidationService,
                                                InteractionService interactionService)
            : base(localizationService)
        {
            _repositoryFactory = repositoryFactory;
            _permissionsValidationService = permissionsValidationService;
            _interactionService = interactionService;
        }

        #endregion // Constructor

        #region DialogSelectMenuElementBase

        /// <summary>
        /// Return the message of element
        /// </summary>
        /// <returns>Message</returns>
        public override async Task<EmbedBuilder> GetMessage()
        {
            var builder = new EmbedBuilder().WithTitle(CommandContext.Guild.Name);

            var configuration = new StringBuilder();
            configuration.Append(LocalizationGroup.GetText("AdminRole", "Administrator role"));
            configuration.Append(": ");

            var administrationRoleId = await _repositoryFactory.GetRepository<ServerConfigurationRepository>()
                                                               .GetQuery()
                                                               .Where(obj => obj.DiscordServerId == CommandContext.Guild.Id)
                                                               .Select(obj => obj.DiscordAdministratorRoleId)
                                                               .FirstOrDefaultAsync()
                                                               .ConfigureAwait(false);

            if (administrationRoleId != null
             && CommandContext.Guild.GetRole(administrationRoleId.Value) is { } administrationRole)
            {
                configuration.AppendLine(administrationRole.Mention);
            }
            else
            {
                configuration.AppendLine(DiscordEmoteService.GetCrossEmote(CommandContext.Client).ToString());
            }

            return builder;
        }

        /// <summary>
        /// Returning the placeholder
        /// </summary>
        /// <returns>Placeholder</returns>
        public override string GetPlaceholder() => LocalizationGroup.GetText("ChooseAction", "Choose one of the following options...");

        /// <summary>
        /// Returns the select menu entries which should be added to the message
        /// </summary>
        /// <returns>Reactions</returns>
        public override IReadOnlyList<SelectMenuEntryData<bool>> GetEntries()
        {
            return new List<SelectMenuEntryData<bool>>
                   {
                       new()
                       {
                           CommandText = LocalizationGroup.GetText("SetAdminRole", "Set administration role"),
                           Func = async () =>
                                  {
                                      var roleId = await RunSubElement<DiscordRoleSelectionDialogElement, ulong>().ConfigureAwait(false);
                                      if (roleId > 0)
                                      {
                                          _permissionsValidationService.AddOrRefresh(CommandContext.Guild.Id, roleId);

                                          return true;
                                      }

                                      return false;
                                  }
                       },
                       new()
                       {
                           CommandText = LocalizationGroup.GetText("InstallSlashCommands", "Slash command installation"),
                           Func = async () =>
                                  {
                                      IEnumerable<ApplicationCommandProperties> commands = null;

                                      var buildContext = new SlashCommandBuildContext
                                                         {
                                                             Guild = CommandContext.Guild,
                                                             ServiceProvider = CommandContext.ServiceProvider,
                                                             CultureInfo = LocalizationGroup.CultureInfo
                                                         };

                                      foreach (var type in Assembly.Load("Scruffy.Commands")
                                                                   .GetTypes()
                                                                   .Where(obj => typeof(SlashCommandModuleBase).IsAssignableFrom(obj)
                                                                              && obj.IsAbstract == false))
                                      {
                                          var commandModule = (SlashCommandModuleBase)Activator.CreateInstance(type);
                                          if (commandModule != null)
                                          {
                                              commands = commands == null
                                                             ? commandModule.GetCommands(buildContext)
                                                             : commands.Concat(commandModule.GetCommands(buildContext));
                                          }
                                      }

                                      if (commands != null)
                                      {
                                          await CommandContext.Guild
                                                              .BulkOverwriteApplicationCommandsAsync(commands.ToArray())
                                                              .ConfigureAwait(false);
                                      }

                                      return true;
                                  }
                       },
                       new()
                       {
                           CommandText = LocalizationGroup.GetText("UninstallSlashCommands", "Slash command uninstallation"),
                           Func = async () =>
                                  {
                                      await CommandContext.Guild
                                                          .BulkOverwriteApplicationCommandsAsync(Array.Empty<ApplicationCommandProperties>())
                                                          .ConfigureAwait(false);

                                      return true;
                                  }
                       },
                       new()
                       {
                           CommandText = LocalizationGroup.GetText("SetSlashCommandPermissions", "Set Slash command permissions"),
                           Func = async () =>
                                  {
                                      var roleId = await RunSubElement<DiscordRoleSelectionDialogElement, ulong>().ConfigureAwait(false);
                                      if (roleId > 0
                                          && CommandContext.Guild.GetRole(roleId) is { } role)
                                      {
                                          foreach (var module in _interactionService.Modules.Where(obj => _administrationCommands.Contains(obj.SlashGroupName)))
                                          {
                                              await _interactionService.ModifySlashCommandPermissionsAsync(module, CommandContext.Guild, new ApplicationCommandPermission(role, true))
                                                                       .ConfigureAwait(false);
                                          }

                                          return true;
                                      }

                                      return false;
                                  }
                       }
                   };
        }

        /// <summary>
        /// Default case if none of the given buttons is used
        /// </summary>
        /// <returns>Result</returns>
        protected override bool DefaultFunc() => false;

        #endregion // DialogSelectMenuElementBase<bool>
    }
}