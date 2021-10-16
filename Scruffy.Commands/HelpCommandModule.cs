using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;

using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Discord.Attributes;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;

namespace Scruffy.Commands
{
    /// <summary>
    /// Help command
    /// </summary>
    [Group("help")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
    public class HelpCommandModule : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="userManagementService">User management service</param>
        public HelpCommandModule(LocalizationService localizationService, UserManagementService userManagementService)
            : base(localizationService, userManagementService)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Standard help
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <param name="command">Command</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [GroupCommand]
        public Task DefaultHelpAsync(CommandContext commandContext, params string[] command)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   try
                                   {
                                       await new CommandsNextExtension.DefaultHelpModule().DefaultHelpAsync(commandContext, command)
                                                                                          .ConfigureAwait(false);
                                   }
                                   catch (ChecksFailedException)
                                   {
                                       await new CommandsNextExtension.DefaultHelpModule().DefaultHelpAsync(commandContext, null)
                                                                                          .ConfigureAwait(false);
                                   }
                                   catch (CommandNotFoundException)
                                   {
                                       await new CommandsNextExtension.DefaultHelpModule().DefaultHelpAsync(commandContext, null)
                                                                                          .ConfigureAwait(false);
                                   }
                               });
        }

        #endregion // Methods

        #region Overview

        /// <summary>
        /// Overview of the most important commands
        /// </summary>
        [Group("overview")]
        [ModuleLifespan(ModuleLifespan.Transient)]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
        public class HelpOverviewCommandModule : LocatedCommandModuleBase
        {
            #region Fields

            #region FIelds

            /// <summary>
            /// Localization service
            /// </summary>
            private LocalizationService _localizationService;

            #endregion // Fields

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            /// <param name="userManagementService">User management service</param>
            public HelpOverviewCommandModule(LocalizationService localizationService, UserManagementService userManagementService)
                : base(localizationService, userManagementService)
            {
                _localizationService = localizationService;
            }

            #endregion // Constructor

            /// <summary>
            /// Overview
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [GroupCommand]
            [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
            public Task StandardOverview(CommandContext commandContext)
            {
                return PostOverview(commandContext, obj2 => obj2 is HelpOverviewCommandAttribute attribute && attribute.Type.HasFlag(HelpOverviewCommandAttribute.OverviewType.Standard));
            }

            /// <summary>
            /// Overview
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("all")]
            [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
            public Task AllOverview(CommandContext commandContext)
            {
                return PostOverview(commandContext, obj2 => true);
            }

            /// <summary>
            /// Overview
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("admin")]
            [RequireAdministratorPermissions]
            [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
            public Task AdminOverview(CommandContext commandContext)
            {
                return PostOverview(commandContext, obj2 => obj2 is HelpOverviewCommandAttribute attribute && attribute.Type.HasFlag(HelpOverviewCommandAttribute.OverviewType.Administration));
            }

            /// <summary>
            /// Overview
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("developer")]
            [RequireAdministratorPermissions]
            [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
            public Task DeveloperOverview(CommandContext commandContext)
            {
                return PostOverview(commandContext, obj2 => obj2 is HelpOverviewCommandAttribute attribute && attribute.Type.HasFlag(HelpOverviewCommandAttribute.OverviewType.Developer));
            }

            /// <summary>
            /// Overview
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <param name="filterExpression">Filtering the commands</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            public Task PostOverview(CommandContext commandContext, Func<Attribute, bool> filterExpression)
            {
                return InvokeAsync(commandContext,
                                   async commandContextContainer =>
                                   {
                                       var localizationGroup = _localizationService.GetGroup(nameof(HelpCommandFormatter));

                                       var builder = new DiscordEmbedBuilder()
                                                         .WithTitle(LocalizationGroup.GetText("HelpCommandOverviewTitle", "Overview of the most important commands"))
                                                         .WithDescription(LocalizationGroup.GetText("HelpCommandOverviewDescription", "The following message is an overview of the most important commands of the given category."))
                                                         .WithColor(DiscordColor.Green)
                                                         .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/ef1f3e1f3f40100fb3750f8d7d25c657.png?size=64")
                                                         .WithTimestamp(DateTime.Now);

                                       var formatter = new HelpCommandFormatter(commandContext, _localizationService);

                                       var fieldCounter = 0;
                                       var builderCounter = 1;

                                       foreach (var topLevelCommandGroup in commandContext.CommandsNext
                                                                                          .RegisteredCommands
                                                                                          .Where(obj => obj.Key == obj.Value.Name)
                                                                                          .Select(obj => obj.Value)
                                                                                          .OfType<CommandGroup>()
                                                                                          .Where(obj => obj.CustomAttributes.Any(filterExpression)))
                                       {
                                           var stringBuilder = new StringBuilder();

                                           async Task AddField()
                                           {
                                               if (fieldCounter == 6)
                                               {
                                                   if (builderCounter == 1)
                                                   {
                                                       builder.WithTitle(builder.Title + " #" + builderCounter);
                                                   }

                                                   builderCounter++;

                                                   await commandContextContainer.Message
                                                                                .RespondAsync(builder)
                                                                                .ConfigureAwait(false);
                                                   fieldCounter = 0;

                                                   builder = new DiscordEmbedBuilder();
                                                   builder.WithTitle(LocalizationGroup.GetText("HelpCommandOverviewTitle", "Overview of the most important commands") + " #" + builderCounter)
                                                          .WithColor(DiscordColor.Green)
                                                          .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/ef1f3e1f3f40100fb3750f8d7d25c657.png?size=64")
                                                          .WithTimestamp(DateTime.Now);
                                               }

                                               if (stringBuilder.Length > 0)
                                               {
                                                   builder.AddField(localizationGroup.GetText(topLevelCommandGroup.Name, topLevelCommandGroup.Name + " commands"), stringBuilder.ToString());

                                                   stringBuilder = new StringBuilder();
                                               }

                                               fieldCounter++;
                                           }

                                           async Task ProcessCommands(IEnumerable<Command> commands)
                                           {
                                               foreach (var command in commands.Where(obj => obj.CustomAttributes.Any(filterExpression)))
                                               {
                                                   if (command is CommandGroup commandGroup)
                                                   {
                                                       await ProcessCommands(commandGroup.Children).ConfigureAwait(false);
                                                   }
                                                   else if ((await command.RunChecksAsync(commandContext, true).ConfigureAwait(false)).Any() == false)
                                                   {
                                                       await formatter.AddCommand(command,
                                                                                  async sb =>
                                                                                  {
                                                                                      var currentLine = sb.ToString();
                                                                                      if (currentLine.Length + stringBuilder.Length > 1024)
                                                                                      {
                                                                                          await AddField().ConfigureAwait(false);
                                                                                      }

                                                                                      stringBuilder.Append(sb);
                                                                                  })
                                                                      .ConfigureAwait(false);
                                                   }
                                               }
                                           }

                                           await ProcessCommands(topLevelCommandGroup.Children).ConfigureAwait(false);

                                           await AddField().ConfigureAwait(false);
                                       }

                                       await commandContextContainer.Message
                                                                    .RespondAsync(builder)
                                                                    .ConfigureAwait(false);
                                   });
            }
        }

        #endregion // Overview
    }
}