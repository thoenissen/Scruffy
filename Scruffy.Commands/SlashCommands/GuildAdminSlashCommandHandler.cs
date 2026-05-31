using Discord;
using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.Guild;
using Scruffy.Services.Guild.Modals;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Guild administration commands
/// </summary>
[Group("guild-admin", "Guild administration commands")]
[DefaultMemberPermissions(GuildPermission.Administrator)]
public class GuildAdminSlashCommandHandler : SlashCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public GuildCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Guild configuration
    /// </summary>
    /// <param name="type">Type</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("configuration", "Guild configuration")]
    public async Task Configuration([Summary("Type", "Configuration type")] GuildAdminConfigurationType type)
    {
        switch (type)
        {
            case GuildAdminConfigurationType.General:
                {
                    await CommandHandler.ConfigureGuild(Context)
                                        .ConfigureAwait(false);
                }
                break;

            case GuildAdminConfigurationType.VoiceActivity:
                {
                    await CommandHandler.ConfigureVoiceActivityRoles(Context)
                                        .ConfigureAwait(false);
                }
                break;

            case GuildAdminConfigurationType.MessageActivity:
                {
                    await CommandHandler.ConfigureMessageActivityRoles(Context)
                                        .ConfigureAwait(false);
                }
                break;

            case GuildAdminConfigurationType.NotificationChannels:
                {
                    await CommandHandler.ConfigureNotificationChannel(Context)
                                        .ConfigureAwait(false);
                }
                break;

            case GuildAdminConfigurationType.Overviews:
                {
                    await CommandHandler.ConfigureOverviewMessages(Context)
                                        .ConfigureAwait(false);
                }
                break;

            case GuildAdminConfigurationType.GuildWarsItems:
                {
                    await CommandHandler.ConfigureItem(Context)
                                        .ConfigureAwait(false);
                }
                break;

            case GuildAdminConfigurationType.User:
                {
                    await CommandHandler.ConfigureUser(Context)
                                        .ConfigureAwait(false);
                }
                break;

            case GuildAdminConfigurationType.Ranks:
                {
                    await CommandHandler.ConfigureRanks(Context)
                                        .ConfigureAwait(false);
                }
                break;

            case GuildAdminConfigurationType.SpecialRanks:
                {
                    await CommandHandler.ConfigureSpecialRanks(Context)
                                        .ConfigureAwait(false);
                }
                break;

            default:
                {
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
        }
    }

    /// <summary>
    /// Overview
    /// </summary>
    /// <param name="type">Type</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("overview", "Guild overview")]
    public async Task Configuration([Summary("Type", "Overview type")] GuildAdminOverviewType type)
    {
        switch (type)
        {
            case GuildAdminOverviewType.Ranking:
                {
                    await CommandHandler.PostRankingOverview(Context)
                                        .ConfigureAwait(false);
                }
                break;

            case GuildAdminOverviewType.SpecialRanks:
                {
                    await CommandHandler.PostSpecialRankOverview(Context)
                                        .ConfigureAwait(false);
                }
                break;

            case GuildAdminOverviewType.Worlds:
                {
                    await CommandHandler.PostWorldsOverview(Context)
                                        .ConfigureAwait(false);
                }
                break;

            default:
                {
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
        }
    }

    /// <summary>
    /// Overview
    /// </summary>
    /// <param name="type">Type</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("export", "Export data")]
    public async Task Export([Summary("Type", "Data type")] DataType type)
    {
        switch (type)
        {
            case DataType.Stash:
                {
                    await Context.RespondWithModalAsync<GuildExportStashModalData>(GuildExportStashModalData.CustomId)
                                 .ConfigureAwait(false);
                }
                break;

            case DataType.Upgrades:
                {
                    await Context.RespondWithModalAsync<GuildExportUpgradesModalData>(GuildExportUpgradesModalData.CustomId)
                                 .ConfigureAwait(false);
                }
                break;

            case DataType.LoginActivity:
                {
                    await CommandHandler.ExportLoginActivity(Context)
                                        .ConfigureAwait(false);
                }
                break;

            case DataType.Representation:
                {
                    await CommandHandler.ExportRepresentation(Context)
                                        .ConfigureAwait(false);
                }
                break;

            case DataType.Members:
                {
                    await CommandHandler.ExportMembers(Context)
                                        .ConfigureAwait(false);
                }
                break;

            case DataType.Roles:
                {
                    await CommandHandler.ExportRoles(Context)
                                        .ConfigureAwait(false);
                }
                break;

            case DataType.Points:
                {
                    await Context.RespondWithModalAsync<GuildExportCurrentPointsModalData>(GuildExportCurrentPointsModalData.CustomId)
                                 .ConfigureAwait(false);
                }
                break;

            case DataType.Items:
                {
                    await CommandHandler.ExportItems(Context)
                                        .ConfigureAwait(false);
                }
                break;

            case DataType.Assignments:
                {
                    await CommandHandler.ExportAssignments(Context)
                                        .ConfigureAwait(false);
                }
                break;

            default:
                {
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
        }
    }

    /// <summary>
    /// Personal ranking data
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="type">Type</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("ranking-of", "Personal guild ranking overview")]
    public async Task PostPersonalRankingOverview([Summary("User", "User")] IGuildUser user,
                                                  [Summary("Type", "Visualization type")] GuildRankingVisualizationType type = GuildRankingVisualizationType.Current)
    {
        switch (type)
        {
            case GuildRankingVisualizationType.Current:
                {
                    await CommandHandler.PostPersonalRankingOverview(Context, user)
                                        .ConfigureAwait(false);
                }
                break;

            case GuildRankingVisualizationType.HistoryTypes:
                {
                    await CommandHandler.PostPersonalRankingHistoryTypeOverview(Context, user)
                                        .ConfigureAwait(false);
                }
                break;

            default:
                {
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
        }
    }

    /// <summary>
    /// Personal ranking data compare
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="compareUser">Compare user</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("compare-of", "Personal guild ranking compare overview")]
    public Task PostPersonalRankingCompareOverview([Summary("UserOne", "First user")] IGuildUser user,
                                                   [Summary("UserTwo", "Second user")] IGuildUser compareUser)
    {
        return CommandHandler.PostPersonalCompareOverview(Context, user, compareUser);
    }

    /// <summary>
    /// Execute checks
    /// </summary>
    /// <param name="type">Type</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("check", "Check execution")]
    public async Task Check([Summary("Type", "Check type")] CheckType type)
    {
        switch (type)
        {
            case CheckType.RankAssignment:
                {
                    await CommandHandler.CheckRankAssignments(Context)
                                        .ConfigureAwait(false);
                }
                break;

            case CheckType.ApiKeys:
                {
                    await CommandHandler.CheckApiKeys(Context)
                                        .ConfigureAwait(false);
                }
                break;

            case CheckType.UnknownUsers:
                {
                    await CommandHandler.CheckUnknownUsers(Context)
                                        .ConfigureAwait(false);
                }
                break;

            default:
                {
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
        }
    }

    /// <summary>
    /// Calculate item
    /// </summary>
    /// <param name="itemId">Item</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("calculate-item", "Calculate item value")]
    public async Task CalculateItem(int itemId)
    {
        await CommandHandler.CalculateItem(Context, itemId)
                            .ConfigureAwait(false);
    }

    #endregion // Methods
}