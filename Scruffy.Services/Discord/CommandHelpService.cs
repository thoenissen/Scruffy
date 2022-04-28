using Discord;
using Discord.Interactions;

using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord.Extensions;
using Scruffy.Services.Discord.Interfaces;

namespace Scruffy.Services.Discord;

/// <summary>
/// Proving the discord command help
/// </summary>
public class CommandHelpService : LocatedServiceBase
{
    #region Fields

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
    /// <param name="interactionService">Commands service</param>
    public CommandHelpService(LocalizationService localizationService, InteractionService interactionService)
        : base(localizationService)
    {
        _interactionService = interactionService;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Show command help
    /// </summary>
    /// <param name="context">Context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ShowHelp(IContextContainer context)
    {
        var embedBuilder = new EmbedBuilder().WithColor(Color.DarkBlue)
                                             .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                             .WithTimestamp(DateTime.Now)
                                             .WithTitle(LocalizationGroup.GetText("BuilderTitleGeneral", "Scruffy - Command help"));

        var commands = new List<(string Name, string Command)>();

        foreach (var command in _interactionService.Modules
                                                   .SelectMany(obj => obj.SlashCommands))
        {
            if ((command.DefaultMemberPermissions == null || context.Member.GuildPermissions.Has(command.DefaultMemberPermissions.Value))
                && (command.Module.DefaultMemberPermissions == null || context.Member.GuildPermissions.Has(command.Module.DefaultMemberPermissions.Value)))
            {
                if (command.Module.SlashGroupName == null)
                {
                    commands.Add((command.Name, command.Name));
                }
                else
                {
                    commands.Add((command.Module.SlashGroupName, command.Module.SlashGroupName + " " + command.Name));
                }
            }
        }

        var modules = new Dictionary<string, (List<string> Commands, bool IsOversized)>();

        foreach (var module in commands.GroupBy(obj => obj.Name))
        {
            var s = LocalizationGroup.GetText(module.Key, module.Key);

            modules.Add(s, //module.Key,
                        (module.Select(obj => obj.Command)
                               .Distinct()
                               .ToList(),
                         module.Any(obj => obj.Command.Length > 18) || s.Length > 18
                         ));
        }

        embedBuilder.WithDescription(LocalizationGroup.GetText("BuilderDescriptionGeneral", "The following commands are available. Further information is displayed when entering the command."));

        foreach (var (module, (moduleCommands, isOversized)) in modules.OrderByDescending(obj => obj.Value.IsOversized)
                                                                       .ThenByDescending(obj => obj.Value.Commands.Count))
        {
            embedBuilder.AddField(Format.Bold(module),
                                  "```" + Environment.NewLine + string.Join(Environment.NewLine, moduleCommands.OrderBy(obj => obj)) + "```",
                                  isOversized == false);
        }

        await context.ReplyAsync(embed: embedBuilder.Build())
                     .ConfigureAwait(false);
    }

    #endregion // Methods
}