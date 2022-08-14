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
    /// Joining an appointment
    /// </summary>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("join", "Join an appointment")]
    public Task Join([Summary("Name", "Name of the appointment")]string name) => CommandHandler.Join(Context, name, true);

    /// <summary>
    /// Leaving an appointment
    /// </summary>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("leave", "Leave an appointment")]
    public async Task Leave([Summary("Name", "Name of the appointment")]string name)
    {
        var message = await Context.DeferProcessing()
                                   .ConfigureAwait(false);

        if (await CommandHandler.Leave(Context, name)
                                .ConfigureAwait(false))
        {
            await message.DeleteAsync()
                         .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Daily logs
    /// </summary>
    /// <param name="day">Day</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("logs", "Creates a listing of all logs of the given day")]
    public async Task Logs([Summary("day", "Day of the logs (yyyy-MM-dd)")]string day = null)
    {
        await Context.DeferAsync()
                     .ConfigureAwait(false);

        await LogCommandHandler.Logs(Context, DpsReportType.Raid, day)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Post guides overview
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("guides", "Show raid guides")]
    public Task Guides() => CommandHandler.Guides(Context);

    /// <summary>
    /// Configure your Raid Ready Roles
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("roles", "Manage your Raid-Ready Roles")]
    public Task Roles() => CommandHandler.Roles(Context);

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