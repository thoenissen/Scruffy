using Discord;
using Discord.Interactions;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Discord;
using Scruffy.Services.GuildWars2;
using Scruffy.Services.Raid;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Raid slash commands
/// </summary>
[Group("raid-admin", "Raid commands")]
[DefaultMemberPermissions(GuildPermission.Administrator)]
public class RaidAdminSlashCommandModule : SlashCommandModuleBase
{
    #region Enumerations

    /// <summary>
    /// Configuration types
    /// </summary>
    public enum ConfigurationType
    {
        [ChoiceDisplay("Appointments")]
        Appointments,
        [ChoiceDisplay("Experience levels")]
        ExperienceLevels,
        [ChoiceDisplay("Templates")]
        Templates
    }

    /// <summary>
    /// Overview types
    /// </summary>
    public enum OverviewType
    {
        [ChoiceDisplay("Participation")]
        Participation,

        [ChoiceDisplay("Levels")]
        Levels,

        [ChoiceDisplay("Logs")]
        Logs,

        [ChoiceDisplay("Current line up")]
        CurrentLineUp,
    }

    #endregion // Enumerations

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
    /// Raid configuration
    /// </summary>
    /// <param name="type">Type of the configuration</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("configuration", "Starting the raid configuration")]
    public async Task Configuration([Summary("Type", "Type of the configuration assistant to be started")]ConfigurationType type)
    {
        switch (type)
        {
            case ConfigurationType.Appointments:
                {
                    await CommandHandler.AppointmentConfiguration(Context)
                                        .ConfigureAwait(false);
                }
                break;
            case ConfigurationType.ExperienceLevels:
                {
                    await CommandHandler.LevelsConfiguration(Context)
                                        .ConfigureAwait(false);
                }
                break;
            case ConfigurationType.Templates:
                {
                    await CommandHandler.TemplatesConfiguration(Context)
                                        .ConfigureAwait(false);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    /// <summary>
    /// Joining an appointment
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("join-user", "Register a user to an appointment")]
    public Task Join([Summary("User")]IGuildUser user,
                     [Summary("Name", "Name of the appointment")]string name) => CommandHandler.Join(Context, user, name);

    /// <summary>
    /// Leaving an appointment
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("leave-user", "Deregister a user to an appointment")]
    public async Task Leave([Summary("User")]IGuildUser user,
                            [Summary("Name", "Name of the appointment")]string name)
    {
        var message = await Context.DeferProcessing()
                                   .ConfigureAwait(false);

        await CommandHandler.Leave(Context, user, name)
                            .ConfigureAwait(false);

        await message.DeleteAsync()
                     .ConfigureAwait(false);
    }

    /// <summary>
    /// Commiting the current raid appointment
    /// </summary>
    /// <param name="name">Alias name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("set-template", "Set raid template")]
    public Task SetTemplate([Summary("Name", "Name of the appointment")]string name) => CommandHandler.SetTemplate(Context, name);

    /// <summary>
    /// Commiting the current raid appointment
    /// </summary>
    /// <param name="name">Alias name</param>
    /// <param name="count">Count</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("set-group-count", "Set group count")]
    public async Task SetTemplate([Summary("Name", "Name of the appointment")]string name,
                                  [Summary("Count", "Group count")]int count)
    {
        await Context.DeferAsync()
                     .ConfigureAwait(false);

        await CommandHandler.SetGroupCount(Context, name, count)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Set experience levels to players
    /// </summary>
    /// <param name="role">Alias name</param>
    /// <param name="user">Users</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("set-experience-level", "Assign a experience to the given users.")]
    public Task SetExperienceLevel([Summary("Role", "The role which should be assigned")]string role,
                                   [Summary("User", "The user who should be assigned the role.")]IGuildUser user) => CommandHandler.SetExperienceLevel(Context, role, user);

    /// <summary>
    /// Commiting the current raid appointment
    /// </summary>
    /// <param name="name">Alias name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("commit", "Commit raid appointment")]
    public Task Commit([Summary("Name", "Name of the appointment")]string name) => CommandHandler.Commit(Context, name);

    /// <summary>
    /// Post a overview of the selected type
    /// </summary>
    /// <param name="type">Type of the configuration</param>
    /// <param name="raidWeekDay">Day of the raid week</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("overview", "Overview of the given data type")]
    public async Task Overview([Summary("Type", "Type of the data which should be visualized")]OverviewType type, [Summary("day-of-raid-week", "Day of the raid week")]string raidWeekDay = null)
    {
        await Context.DeferProcessing()
                     .ConfigureAwait(false);

        switch (type)
        {
            case OverviewType.Participation:
                {
                    await CommandHandler.PostParticipationOverview(Context)
                                        .ConfigureAwait(false);
                }
                break;
            case OverviewType.Levels:
                {
                    await CommandHandler.PostExperienceLevelOverview(Context)
                                        .ConfigureAwait(false);
                }
                break;
            case OverviewType.Logs:
                {
                    await LogCommandHandler.PostGuildRaidSummary(Context, raidWeekDay)
                                           .ConfigureAwait(false);
                }
                break;
            case OverviewType.CurrentLineUp:
                {
                    await CommandHandler.PostCurrentLineUp(Context)
                                        .ConfigureAwait(false);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    #endregion // Commands

    #region SlashCommandModuleBase

    /// <inheritdoc/>
    public override IEnumerable<ApplicationCommandProperties> GetCommands(SlashCommandBuildContext buildContext)
    {
        var repositoryFactory = buildContext.ServiceProvider
                                            .GetRequiredService<RepositoryFactory>();

        var levels = repositoryFactory.GetRepository<RaidExperienceLevelRepository>()
                                      .GetQuery()
                                      .Select(obj => new
                                                     {
                                                         obj.Description,
                                                         obj.AliasName
                                                     })
                                      .ToList();

        var levelChoices = levels.Select(obj => new ApplicationCommandOptionChoiceProperties
                                                {
                                                    Value = obj.AliasName,
                                                    Name = obj.Description
                                                })
                                 .ToList();

        var appointments = repositoryFactory.GetRepository<RaidDayConfigurationRepository>()
                                            .GetQuery()
                                            .Select(obj => new
                                                           {
                                                               obj.Day,
                                                               obj.AliasName
                                                           })
                                            .ToList();

        var appointmentChoices = appointments.Select(obj => new ApplicationCommandOptionChoiceProperties
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
                        && obj.Name.Value == "raid-admin")
                       {
                           foreach (var option in obj.Options.Value)
                           {
                               if (option.Name is "commit"
                                               or "set-template"
                                               or "set-group-count"
                                               or "join-user"
                                               or "leave-user")
                               {
                                   foreach (var innerOption in option.Options.Where(obj2 => obj2.Name == "name"))
                                   {
                                       innerOption.Choices = appointmentChoices;
                                   }
                               }
                               else if (option.Name is "set-experience-level")
                               {
                                   foreach (var innerOption in option.Options.Where(obj2 => obj2.Name == "role"))
                                   {
                                       innerOption.Choices = levelChoices;
                                   }
                               }
                           }
                       }

                       return obj;
                   });
    }

    #endregion // SlashCommandModuleBase
}