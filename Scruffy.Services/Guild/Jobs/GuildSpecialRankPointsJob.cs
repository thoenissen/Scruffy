using System.Data;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Enumerations.Guild;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Guild.Jobs;

/// <summary>
/// Calculating the daily points
/// </summary>
public class GuildSpecialRankPointsJob : LocatedAsyncJob
{
    #region LocatedAsyncJob

    /// <summary>
    /// Executes the job
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task ExecuteOverrideAsync()
    {
        var serviceProvider = Core.ServiceProviderContainer.Current.GetServiceProvider();
        await using (serviceProvider.ConfigureAwait(false))
        {
            var client = serviceProvider.GetService<DiscordSocketClient>();
            var userManagementService = serviceProvider.GetService<UserManagementService>();

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                foreach (var configuration in dbFactory.GetRepository<GuildSpecialRankConfigurationRepository>()
                                                       .GetQuery()
                                                       .Where(obj => obj.IsDeleted == false)
                                                       .Select(obj => new
                                                                      {
                                                                          obj.Id,
                                                                          obj.Guild.DiscordServerId,
                                                                          obj.MaximumPoints,
                                                                          Roles = obj.GuildSpecialRankRoleAssignments
                                                                                     .Select(obj2 => new
                                                                                                     {
                                                                                                         obj2.DiscordRoleId,
                                                                                                         obj2.Points
                                                                                                     }),
                                                                          IgnoreRoles = obj.GuildSpecialRankIgnoreRoleAssignments
                                                                                           .Select(obj2 => obj2.DiscordRoleId)
                                                                      })
                                                       .ToList())
                {
                    var points = new List<(ulong UserId, double Points)>();

                    var guild = client.GetGuild(configuration.DiscordServerId);

                    await foreach (var users in guild.GetUsersAsync())
                    {
                        foreach (var user in users)
                        {
                            if (configuration.IgnoreRoles.Any(obj => user.RoleIds.Any(obj2 => obj2 == obj)) == false)
                            {
                                foreach (var role in configuration.Roles)
                                {
                                    if (user.RoleIds.Any(obj => obj == role.DiscordRoleId))
                                    {
                                        points.Add((user.Id, role.Points));
                                    }
                                }
                            }
                        }
                    }

                    var discordUserAccountIds = points.Select(obj => obj.UserId)
                                                      .ToList();

                    var accounts = dbFactory.GetRepository<DiscordAccountRepository>()
                                         .GetQuery()
                                         .Where(obj => discordUserAccountIds.Contains(obj.Id))
                                         .ToDictionary(obj => obj.Id, obj => obj.UserId);

                    foreach (var pointsPerUser in points.GroupBy(obj => obj.UserId))
                    {
                        await userManagementService.CheckDiscordAccountAsync(pointsPerUser.Key)
                                                   .ConfigureAwait(false);

                        var transaction = dbFactory.BeginTransaction(IsolationLevel.RepeatableRead);
                        await using (transaction.ConfigureAwait(false))
                        {
                            if (accounts.TryGetValue(pointsPerUser.Key, out var userId))
                            {
                                if (await dbFactory.GetRepository<GuildSpecialRankPointsRepository>()
                                                   .AddPoints(configuration.Id, configuration.MaximumPoints, userId, pointsPerUser.Select(obj => obj.Points).ToList())
                                                   .ConfigureAwait(false))
                                {
                                    await transaction.CommitAsync()
                                                     .ConfigureAwait(false);
                                }
                            }
                        }
                    }
                }

                var channels = dbFactory.GetRepository<GuildChannelConfigurationRepository>()
                                        .GetQuery()
                                        .Select(obj => obj);

                foreach (var configuration in dbFactory.GetRepository<GuildSpecialRankConfigurationRepository>()
                                                       .GetQuery()
                                                       .Where(obj => obj.IsDeleted == false)
                                                       .Select(obj => new
                                                                      {
                                                                          obj.Id,
                                                                          obj.Description,
                                                                          obj.Guild.DiscordServerId,
                                                                          obj.DiscordRoleId,
                                                                          ChannelId = channels.Where(obj2 => obj2.GuildId == obj.GuildId
                                                                                                          && obj2.Type == GuildChannelConfigurationType.SpecialRankRankChange)
                                                                                              .Select(obj2 => (ulong?)obj2.DiscordChannelId)
                                                                                              .FirstOrDefault(),
                                                                          Users = obj.GuildSpecialRankPoints
                                                                                     .Where(obj2 => obj2.Points > obj.RemoveThreshold)
                                                                                     .SelectMany(obj2 => obj2.User
                                                                                                             .DiscordAccounts
                                                                                                             .Select(obj3 => new
                                                                                                                             {
                                                                                                                                 obj3.Id,
                                                                                                                                 IsGrantRole = obj2.Points > obj.GrantThreshold
                                                                                                                             })),
                                                                          IgnoreRoles = obj.GuildSpecialRankIgnoreRoleAssignments
                                                                                           .Select(obj2 => obj2.DiscordRoleId)
                                                                      })
                                                       .ToList())
                {
                    var actions = new List<(bool IsGrant, IGuildUser User, ulong RoleId)>();

                    var guild = client.GetGuild(configuration.DiscordServerId);

                    await foreach (var users in guild.GetUsersAsync()
                                                     .ConfigureAwait(false))
                    {
                        foreach (var user in users)
                        {
                            if (configuration.IgnoreRoles.Any(obj => user.RoleIds.Any(obj2 => obj2 == obj)) == false)
                            {
                                var isRoleAssigned = user.RoleIds.Any(obj => obj == configuration.DiscordRoleId);
                                if (isRoleAssigned)
                                {
                                    if (configuration.Users.Any(obj => obj.Id == user.Id) == false)
                                    {
                                        actions.Add((false, user, configuration.DiscordRoleId));
                                    }
                                }
                                else if (configuration.Users.FirstOrDefault(obj => obj.Id == user.Id)?.IsGrantRole == true)
                                {
                                    actions.Add((true, user, configuration.DiscordRoleId));
                                }
                            }
                        }
                    }

                    if (actions.Count > 0)
                    {
                        foreach (var action in actions)
                        {
                            try
                            {
                                var role = action.User.Guild.GetRole(action.RoleId);
                                if (role != null)
                                {
                                    if (action.IsGrant)
                                    {
                                        await action.User
                                                    .AddRoleAsync(role)
                                                    .ConfigureAwait(false);

                                        LoggingService.AddJobLogEntry(LogEntryLevel.Warning, nameof(GuildSpecialRankPointsJob), $"Role granted: configuration {configuration.Id}; user: {action.User.Id}; role: {role.Id}");
                                    }
                                    else
                                    {
                                        await action.User
                                                    .RemoveRoleAsync(role)
                                                    .ConfigureAwait(false);

                                        LoggingService.AddJobLogEntry(LogEntryLevel.Warning, nameof(GuildSpecialRankPointsJob), $"Role revoked: configuration {configuration.Id}; user: {action.User.Id}; role: {role.Id}");
                                    }
                                }
                                else
                                {
                                    LoggingService.AddJobLogEntry(LogEntryLevel.Error, nameof(GuildSpecialRankPointsJob), "Role assignment", $"Unknown role {action.RoleId} at {action.User.Guild.Id}", null);
                                }
                            }
                            catch (Exception ex)
                            {
                                LoggingService.AddJobLogEntry(LogEntryLevel.Error, nameof(GuildSpecialRankPointsJob), "Role assignment", null, ex);
                            }
                        }

                        if (configuration.ChannelId != null)
                        {
                            var builder = new EmbedBuilder();
                            builder.WithTitle(LocalizationGroup.GetText("RoleAssignment", "Role assignment"));
                            builder.WithDescription($"{configuration.Description} ({guild.GetRole(configuration.DiscordRoleId).Mention})");
                            builder.WithColor(Color.DarkBlue);

                            if (actions.Any(obj => obj.IsGrant == false))
                            {
                                var stringBuilder = new StringBuilder();

                                foreach (var action in actions.Where(obj => obj.IsGrant == false))
                                {
                                    stringBuilder.AppendLine(DiscordEmoteService.GetBulletEmote(client) + " " + action.User.Mention);
                                }

                                builder.AddField(LocalizationGroup.GetText("RemovedRoles", "Removed roles"), stringBuilder.ToString());
                            }

                            if (actions.Any(obj => obj.IsGrant))
                            {
                                var stringBuilder = new StringBuilder();

                                foreach (var action in actions.Where(obj => obj.IsGrant))
                                {
                                    stringBuilder.AppendLine(DiscordEmoteService.GetBulletEmote(client) + " " + action.User.Mention);
                                }

                                builder.AddField(LocalizationGroup.GetText("GrantedRoles", "Granted roles"), stringBuilder.ToString());
                            }

                            var channel = await client.GetChannelAsync(configuration.ChannelId.Value)
                                                      .ConfigureAwait(false);

                            if (channel is IMessageChannel messageChannel)
                            {
                                await messageChannel.SendMessageAsync(embed: builder.Build())
                                                    .ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            LoggingService.AddJobLogEntry(LogEntryLevel.Warning, nameof(GuildSpecialRankPointsJob), $"No notification channel for configuration {configuration.Id}");
                        }
                    }
                }
            }
        }
    }

    #endregion // LocatedAsyncJob
}