using Discord;
using Discord.Commands;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord.Extensions;

namespace Scruffy.Services.Discord
{
    /// <summary>
    /// Proving the discord command help
    /// </summary>
    public class CommandHelpService : LocatedServiceBase
    {
        #region Fields

        /// <summary>
        /// Command service
        /// </summary>
        private readonly CommandService _commandService;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="commandService">Commands service</param>
        public CommandHelpService(LocalizationService localizationService, CommandService commandService)
            : base(localizationService)
        {
            _commandService = commandService;
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Show command help
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <param name="commandName">Command name</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ShowHelp(CommandContextContainer commandContext, string commandName)
        {
            var embedBuilder = new EmbedBuilder().WithColor(Color.DarkBlue)
                                                 .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                                 .WithTimestamp(DateTime.Now)
                                                 .WithTitle(LocalizationGroup.GetText("BuilderTitleGeneral", "Scruffy - Command help"));

            var commands = new List<(string Name, string Command)>();

            foreach (var mainModule in _commandService.Modules
                                                  .Where(obj => obj.Name != "debug"
                                                             && obj.Name != "help"
                                                             && obj.Parent == null))
            {
                // Checking preconditions
                async Task<bool> ArePreconditionsSatisfied(IEnumerable<PreconditionAttribute> preconditions)
                {
                    var arePreconditionsSatisfied = true;

                    foreach (var precondition in preconditions)
                    {
                        try
                        {
                            if ((await precondition.CheckPermissionsAsync(commandContext, null, commandContext.ServiceProvider).ConfigureAwait(false)).IsSuccess == false)
                            {
                                arePreconditionsSatisfied = false;
                                break;
                            }
                        }
                        catch
                        {
                            arePreconditionsSatisfied = false;
                            break;
                        }
                    }

                    return arePreconditionsSatisfied;
                }

                // Adding commands of module
                async Task AddCommands(ModuleInfo module)
                {
                    if (await ArePreconditionsSatisfied(module.Preconditions).ConfigureAwait(false))
                    {
                        if (module.Commands.Count > 0)
                        {
                            foreach (var command in module.Commands)
                            {
                                if (await ArePreconditionsSatisfied(command.Preconditions).ConfigureAwait(false))
                                {
                                    commands.Add((command.GetMainGroup(), command.GetFullName()));
                                }
                            }
                        }

                        if (module.Submodules != null)
                        {
                            foreach (var submodule in module.Submodules)
                            {
                                await AddCommands(submodule).ConfigureAwait(false);
                            }
                        }
                    }
                }

                await AddCommands(mainModule).ConfigureAwait(false);
            }

            // Filtered command list
            List<(string Name, string Command)> filteredCommands = null;

            if (string.IsNullOrWhiteSpace(commandName))
            {
                filteredCommands = commands;
            }
            else
            {
                while (filteredCommands == null)
                {
                    if (commands.Any(obj => obj.Command.StartsWith(commandName, StringComparison.InvariantCulture)))
                    {
                        filteredCommands = commands.Where(obj => obj.Command.StartsWith(commandName, StringComparison.InvariantCulture)).ToList();
                    }
                    else
                    {
                        var i = commandName.LastIndexOf(' ');
                        if (i == -1)
                        {
                            filteredCommands = commands;
                            commandName = string.Empty;
                        }
                        else
                        {
                            commandName = commandName[..i];
                        }
                    }
                }
            }

            // Grouping by main module
            var modules = new Dictionary<string, (List<string> Commands, bool IsOversized)>();
            foreach (var module in filteredCommands.GroupBy(obj => obj.Name))
            {
                modules.Add(module.Key, (module.Select(obj => obj.Command).Distinct().ToList(), module.Any(obj => obj.Command.Length > 18)));
            }

            // Do we need to display a command help?
            if (modules.Count == 1
             && modules.First().Value.Commands.Count == 1)
            {
                commandName = modules.First().Value.Commands.First();

                var search = _commandService.Search(commandContext, commandName);
                if (search.Commands != null)
                {
                    embedBuilder.WithDescription(LocalizationGroup.GetFormattedText("BuilderCommandListDescription", "The command help of the command `{0}` is displayed below.", commandName));

                    var sb = new StringBuilder();

                    sb.AppendLine(Format.Bold(LocalizationGroup.GetText("BuilderTitleExplanation", "Explanation")));
                    sb.AppendLine(LocalizationGroup.GetText(commandName, string.Empty));
                    sb.AppendLine();
                    sb.AppendLine(Format.Bold(LocalizationGroup.GetText("BuilderTitleUsage", "Usage")));
                    sb.Append("```");

                    foreach (var command in search.Commands)
                    {
                        sb.Append(command.Command.GetFullName());

                        foreach (var parameter in command.Command.Parameters)
                        {
                            sb.Append(' ');
                            sb.Append(parameter.IsOptional ? '[' : '<');
                            sb.Append(LocalizationGroup.GetText(commandName + " " + parameter.Name, parameter.Name));

                            if (parameter.IsRemainder)
                            {
                                sb.Append("...");
                            }

                            sb.Append(parameter.IsOptional ? ']' : '>');
                        }

                        sb.AppendLine();
                    }

                    sb.Append("```");
                    sb.AppendLine();

                    sb.AppendLine(Format.Bold(LocalizationGroup.GetText("BuilderTitleArguments", "Arguments")));

                    foreach (var parameter in search.Commands.SelectMany(obj => obj.Command.Parameters.Select(obj2 => obj2.Name)).Distinct())
                    {
                        sb.Append("`");
                        sb.Append(LocalizationGroup.GetText(commandName + " " + parameter, parameter));

                        if (LocalizationGroup.TryGetText(commandName + " " + parameter + " format", out var format))
                        {
                            sb.Append(' ');
                            sb.Append('(');
                            sb.Append(format);
                            sb.Append(')');
                        }

                        sb.Append("`");

                        if (LocalizationGroup.TryGetText(commandName + " " + parameter + " description", out var text))
                        {
                            sb.Append(": ");
                            sb.AppendLine(text);
                        }
                        else
                        {
                            sb.AppendLine();
                        }
                    }

                    sb.AppendLine();
                    sb.AppendLine(Format.Bold(LocalizationGroup.GetText("BuilderTitleLegend", "Legend")));
                    sb.AppendLine(LocalizationGroup.GetText("BuilderTitleLegendRequired", "`<Argument>`: This argument is required."));
                    sb.AppendLine(LocalizationGroup.GetText("BuilderTitleLegendOptional", "`[Argument]`: This argument is optional."));
                    sb.AppendLine(LocalizationGroup.GetText("BuilderTitleLegendRemainder", "`...`: Spaces and line breaks can be used with this argument."));

                    embedBuilder.AddField("\u200b", sb.ToString());
                }
            }

            if (embedBuilder.Fields.Count == 0)
            {
                if (string.IsNullOrEmpty(commandName) == false)
                {
                    embedBuilder.Description += $" [{commandName}]";
                }

                embedBuilder.WithDescription(LocalizationGroup.GetText("BuilderDescriptionGeneral", "With the command `help <command>` you can get the help of a certain command."));

                foreach (var (module, (moduleCommands, isOversized)) in modules.OrderByDescending(obj => obj.Value.IsOversized).ThenByDescending(obj => obj.Value.Commands.Count))
                {
                    embedBuilder.AddField(Format.Bold(LocalizationGroup.GetText(module, module)),
                                          "```" + string.Join(Environment.NewLine, moduleCommands.OrderBy(obj => obj)) + "```",
                                          isOversized == false);
                }
            }

            await commandContext.Channel
                                .SendMessageAsync(embed: embedBuilder.Build())
                                .ConfigureAwait(false);
        }

        #endregion // Methods
    }
}
