using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildAdministration;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;
using Scruffy.Services.CoreData;

namespace Scruffy.Services.GuildAdministration
{
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
        public override async Task ExecuteAsync()
        {
            await using (var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider())
            {
                var client = serviceProvider.GetService<DiscordClient>();
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

                        var guild = await client.GetGuildAsync(configuration.DiscordServerId)
                                                .ConfigureAwait(false);

                        foreach (var user in await guild.GetAllMembersAsync()
                                                        .ConfigureAwait(false))
                        {
                            if (configuration.IgnoreRoles.Any(obj => user.Roles.Any(obj2 => obj2.Id == obj)) == false)
                            {
                                foreach (var role in configuration.Roles)
                                {
                                    if (user.Roles.Any(obj => obj.Id == role.DiscordRoleId))
                                    {
                                        points.Add((user.Id, role.Points));
                                    }
                                }
                            }
                        }

                        foreach (var pointsPerUser in points.GroupBy(obj => obj.UserId))
                        {
                            await userManagementService.CheckUserAsync(pointsPerUser.Key)
                                                       .ConfigureAwait(false);

                            await using (var transaction = dbFactory.BeginTransaction(IsolationLevel.RepeatableRead))
                            {
                                if (await dbFactory.GetRepository<GuildSpecialRankPointsRepository>()
                                                   .AddPoints(configuration.Id, configuration.MaximumPoints, pointsPerUser.Key, pointsPerUser.Select(obj => obj.Points).ToList())
                                                   .ConfigureAwait(false))
                                {
                                    await transaction.CommitAsync()
                                                     .ConfigureAwait(false);
                                }
                            }
                        }
                    }

                    foreach (var configuration in dbFactory.GetRepository<GuildSpecialRankConfigurationRepository>()
                                                           .GetQuery()
                                                           .Where(obj => obj.IsDeleted == false)
                                                           .Select(obj => new
                                                                          {
                                                                              obj.Id,
                                                                              obj.Description,
                                                                              obj.Guild.DiscordServerId,
                                                                              obj.Guild.NotificationChannelId,
                                                                              obj.DiscordRoleId,
                                                                              Users = obj.GuildSpecialRankPoints
                                                                                         .Where(obj2 => obj2.Points > obj.RemoveThreshold)
                                                                                         .Select(obj2 => new
                                                                                                         {
                                                                                                             obj2.UserId,
                                                                                                             IsGrantRole = obj2.Points > obj.GrantThreshold
                                                                                                         }),
                                                                              IgnoreRoles = obj.GuildSpecialRankIgnoreRoleAssignments
                                                                                               .Select(obj2 => obj2.DiscordRoleId)
                                                                          })
                                                           .ToList())
                    {
                        var actions = new List<(bool IsGrant, DiscordMember User)>();

                        var guild = await client.GetGuildAsync(configuration.DiscordServerId)
                                                .ConfigureAwait(false);

                        foreach (var user in await guild.GetAllMembersAsync()
                                                        .ConfigureAwait(false))
                        {
                            if (configuration.IgnoreRoles.Any(obj => user.Roles.Any(obj2 => obj2.Id == obj)) == false)
                            {
                                var isRoleAssigned = user.Roles.Any(obj => obj.Id == configuration.DiscordRoleId);
                                if (isRoleAssigned)
                                {
                                    if (configuration.Users.Any(obj => obj.UserId == user.Id) == false)
                                    {
                                        actions.Add((false, user));
                                    }
                                }
                                else if (configuration.Users.FirstOrDefault(obj => obj.UserId == user.Id)?.IsGrantRole == true)
                                {
                                    actions.Add((true, user));
                                }
                            }
                        }

                        if (actions.Count > 0)
                        {
                            if (configuration.NotificationChannelId != null)
                            {
                                var builder = new DiscordEmbedBuilder();
                                builder.WithTitle(LocalizationGroup.GetText("RoleAssignment", "Role assignment"));
                                builder.WithDescription($"{configuration.Description} ({guild.GetRole(configuration.DiscordRoleId).Mention})");
                                builder.WithColor(DiscordColor.DarkBlue);

                                if (actions.Any(obj => obj.IsGrant == false))
                                {
                                    var stringBuilder = new StringBuilder();

                                    foreach (var action in actions.Where(obj => obj.IsGrant == false))
                                    {
                                        stringBuilder.AppendLine(DiscordEmojiService.GetBulletEmoji(client) + " " + action.User.Mention);
                                    }

                                    builder.AddField(LocalizationGroup.GetText("RemovedRoles", "Removed roles"), stringBuilder.ToString());
                                }

                                if (actions.Any(obj => obj.IsGrant))
                                {
                                    var stringBuilder = new StringBuilder();

                                    foreach (var action in actions.Where(obj => obj.IsGrant))
                                    {
                                        stringBuilder.AppendLine(DiscordEmojiService.GetBulletEmoji(client) + " " + action.User.Mention);
                                    }

                                    builder.AddField(LocalizationGroup.GetText("GrantedRoles", "Granted roles"), stringBuilder.ToString());
                                }

                                var channel = await client.GetChannelAsync(configuration.NotificationChannelId.Value)
                                                          .ConfigureAwait(false);

                                await channel.SendMessageAsync(builder)
                                             .ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
        }

        #endregion // LocatedAsyncJob
    }
}
