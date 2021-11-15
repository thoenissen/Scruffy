using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;

using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Core.Discord;

/// <summary>
/// Formatting the help messages
/// </summary>
public class HelpCommandFormatter : BaseHelpFormatter
{
    #region Fields

    /// <summary>
    /// Located texts
    /// </summary>
    private LocalizationGroup _localizationGroup;

    /// <summary>
    /// Current command
    /// </summary>
    private Command _currentCommand;

    /// <summary>
    /// Message builder
    /// </summary>
    private DiscordEmbedBuilder _embedBuilder;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Creates a new default help formatter.
    /// </summary>
    /// <param name="ctx">Context in which this formatter is being invoked.</param>
    /// <param name="localizationService">Localization service</param>
    public HelpCommandFormatter(CommandContext ctx, LocalizationService localizationService)
        : base(ctx)
    {
        _localizationGroup = localizationService.GetGroup(GetType().Name);

        _embedBuilder = new DiscordEmbedBuilder().WithTitle(_localizationGroup.GetText("Help", "Help"))
                                                 .WithColor(DiscordColor.MidnightBlue);
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Sets the command this help message will be for.
    /// </summary>
    /// <param name="command">Command for which the help message is being produced.</param>
    /// <returns>This help formatter.</returns>
    public override BaseHelpFormatter WithCommand(Command command)
    {
        _currentCommand = command;

        _embedBuilder.WithDescription($"{Formatter.InlineCode(command.QualifiedName)}: {_localizationGroup.GetText(command.QualifiedName, _localizationGroup.GetText("NoDescription", "No description provided."))}");

        AddCommand(command, sb => Task.Run(() => _embedBuilder.AddField(_localizationGroup.GetText("Arguments", "Arguments"), sb.ToString().Trim()))).Wait();

        return this;
    }

    /// <summary>
    /// Adding a command
    /// </summary>
    /// <param name="command">Command</param>
    /// <param name="onCommand">Adding a command overload</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task AddCommand(Command command, Func<StringBuilder, Task> onCommand)
    {
        if (command.Overloads?.Any() == true)
        {
            var sb = new StringBuilder();

            foreach (var ovl in command.Overloads.OrderByDescending(x => x.Priority))
            {
                sb.Append('`').Append(command.QualifiedName);

                foreach (var arg in ovl.Arguments)
                {
                    sb.Append(arg.IsOptional || arg.IsCatchAll ? " [" : " <")
                      .Append(_localizationGroup.GetText($"{command.QualifiedName} {arg.Name}", arg.Name))
                      .Append(arg.IsCatchAll ? "..." : string.Empty)
                      .Append(arg.IsOptional || arg.IsCatchAll ? ']' : '>');
                }

                sb.Append("` " + _localizationGroup.GetText(command.QualifiedName, string.Empty) + "\n");

                foreach (var arg in ovl.Arguments)
                {
                    sb.Append("` - ")
                      .Append(_localizationGroup.GetText($"{command.QualifiedName} {arg.Name}", arg.Name));

                    if (_localizationGroup.TryGetText($"{command.QualifiedName} {arg.Name} format", out var format))
                    {
                        sb.Append(" (")
                          .Append(format)
                          .Append(')');
                    }

                    sb.Append("`");

                    if (_localizationGroup.TryGetText($"{command.QualifiedName} {arg.Name} description", out var description))
                    {
                        sb.Append(" : ")
                          .Append(description);
                    }

                    sb.Append("\n");
                }
            }

            await onCommand(sb).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Sets the subcommands for this command, if applicable. This method will be called with filtered data.
    /// </summary>
    /// <param name="subcommands">Subcommands for this command group.</param>
    /// <returns>This help formatter.</returns>
    public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
    {
        var sb = new StringBuilder();

        foreach (var command in subcommands)
        {
            sb.Append($"{Formatter.InlineCode(command.QualifiedName)}: {_localizationGroup.GetText(command.QualifiedName, _localizationGroup.GetText("NoDescription", "No description provided."))}\n");
        }

        _embedBuilder.AddField(_currentCommand != null
                                   ? _localizationGroup.GetText("Subcommands", "Subcommands")
                                   : _localizationGroup.GetText("Commands", "Commands"),
                               sb.ToString());

        return this;
    }

    /// <summary>
    /// Construct the help message.
    /// </summary>
    /// <returns>Data for the help message.</returns>
    public override CommandHelpMessage Build()
    {
        if (_currentCommand == null)
        {
            _embedBuilder.WithDescription(_localizationGroup.GetText("GeneralHelp", "Listing all top-level commands and groups. Specify a command to see more information."));
        }

        return new CommandHelpMessage(embed: _embedBuilder.Build());
    }

    #endregion // Properties
}