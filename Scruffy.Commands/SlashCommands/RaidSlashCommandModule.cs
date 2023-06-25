using Discord;
using Discord.Interactions;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Enumerations.DpsReport;
using Scruffy.Services.Discord;
using Scruffy.Services.GuildWars2;
using Scruffy.Services.Raid;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Raid slash commands
/// </summary>
[Group("raid", "Raid commands")]
public class RaidSlashCommandModule : SlashCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public RaidCommandHandler CommandHandler { get; set; }

    /// <summary>
    /// Log command handler
    /// </summary>
    public LogCommandHandler LogCommandHandler { get; set; }

    #endregion // Properties

    #region Commands

    /// <summary>
    /// Configure your prepared raid roles
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("roles", "Manage your prepared raid roles")]
    public Task Roles() => CommandHandler.ConfigureRoles(Context);

    /// <summary>
    /// Configure your prepared special raid roles
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("special-roles", "Manage your prepared special raid roles")]
    public Task SpecialRoles() => CommandHandler.ConfigureSpecialRoles(Context);

    #endregion // Commands

    #region SlashCommandModuleBase

    /// <summary>
    /// Creates a list of all commands
    /// </summary>
    /// <remarks>Only the <see cref="SlashCommandBuildContext"/> is available and not the command context during this method.</remarks>
    /// <param name="buildContext">Build context</param>
    /// <returns>List of commands</returns>
    public override IEnumerable<ApplicationCommandProperties> GetCommands(SlashCommandBuildContext buildContext)
    {
        var repositoryFactory = buildContext.ServiceProvider
                                            .GetRequiredService<RepositoryFactory>();

        var appointments = repositoryFactory.GetRepository<RaidDayConfigurationRepository>()
                                            .GetQuery()
                                            .Select(obj => new
                                                           {
                                                               obj.Day,
                                                               obj.AliasName
                                                           })
                                            .ToList();

        var choices = appointments.Select(obj => new ApplicationCommandOptionChoiceProperties
                                                 {
                                                     Value = obj.AliasName,
                                                     Name = buildContext.CultureInfo.DateTimeFormat.GetDayName(obj.Day)
                                                 })
                                  .ToList();

        return base.GetCommands(buildContext)
                   .OfType<SlashCommandProperties>()
                   .Select(obj =>
                           {
                               if (obj.Options.IsSpecified
                                && obj.Name.IsSpecified
                                && obj.Name.Value == "raid")
                               {
                                   foreach (var option in obj.Options.Value.Where(option => option.Name is "join" or "leave"))
                                   {
                                       option.Options[0].Choices = choices;
                                   }
                               }

                               return obj;
                           });
    }

    #endregion // SlashCommandModuleBase
}