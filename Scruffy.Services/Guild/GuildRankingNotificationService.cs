using Discord;
using Discord.Rest;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Enumerations.Guild;
using Scruffy.Services.Core;

namespace Scruffy.Services.Guild
{
    /// <summary>
    /// Guild ranking notification service
    /// </summary>
    public class GuildRankingNotificationService : SingletonLocatedServiceBase
    {
        #region Fields

        /// <summary>
        /// Discord client
        /// </summary>
        private DiscordSocketClient _discordClient;

        #endregion // Fields

        #region Methods

        /// <summary>
        /// Logged in
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private Task OnConnected()
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var ranks = dbFactory.GetRepository<GuildDiscordActivityPointsAssignmentRepository>()
                                     .GetQuery()
                                     .Select(obj => obj);

                foreach (var guild in dbFactory.GetRepository<GuildChannelConfigurationRepository>()
                                               .GetQuery()
                                               .Where(obj => obj.Type == GuildChannelConfigurationType.GuildRankChanges)
                                               .Select(obj => new
                                               {
                                                   obj.Guild.DiscordServerId,
                                                   obj.DiscordChannelId,
                                                   VoiceRoles = ranks.Where(obj2 => obj2.GuildId == obj.GuildId
                                                                                 && obj2.Type == DiscordActivityPointsType.Voice)
                                                                                    .Select(obj2 => obj2.RoleId)
                                                                                    .ToList(),
                                                   MessageRoles = ranks.Where(obj2 => obj2.GuildId == obj.GuildId
                                                                                 && obj2.Type == DiscordActivityPointsType.Message)
                                                                                      .Select(obj2 => obj2.RoleId)
                                                                                      .ToList()
                                               }))
                {
                    var unused = StartAuditLogWatchAsync(guild.DiscordServerId, guild.DiscordChannelId, guild.VoiceRoles, guild.MessageRoles);
                }
            }

            _discordClient.LoggedIn -= OnConnected;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Start the audit log watch
        /// </summary>
        /// <param name="serverId">Server id</param>
        /// <param name="channelId">Channel id</param>
        /// <param name="voiceRoles">Voice roles</param>
        /// <param name="messageRoles">Message roles</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task StartAuditLogWatchAsync(ulong serverId, ulong channelId, List<ulong> voiceRoles, List<ulong> messageRoles)
        {
            var guild = _discordClient.GetGuild(serverId);
            if (await _discordClient.GetChannelAsync(channelId)
                                    .ConfigureAwait(false) is ITextChannel channel)
            {
                var lastLogId = 0ul;

                await foreach (var logs in guild.GetAuditLogsAsync(1, actionType: ActionType.MemberRoleUpdated)
                                               .ConfigureAwait(false))
                {
                    lastLogId = logs.FirstOrDefault()?.Id ?? 0;
                }

                while (true)
                {
                    try
                    {
                        await foreach (var logs in guild.GetAuditLogsAsync(10, actionType: ActionType.MemberRoleUpdated)
                                                        .ConfigureAwait(false))
                        {
                            foreach (var log in logs.Where(obj => obj.Id > lastLogId))
                            {
                                if (log.Data is MemberRoleAuditLogData logData)
                                {
                                    foreach (var editInfo in logData.Roles
                                                                    .Where(obj => obj.Added))
                                    {
                                        if (voiceRoles.Contains(editInfo.RoleId)
                                         || messageRoles.Contains(editInfo.RoleId))
                                        {
                                            await channel.SendMessageAsync($"The role {guild.GetRole(editInfo.RoleId).Mention} has been assigned to {logData.Target.Mention}.", allowedMentions: AllowedMentions.None)
                                                         .ConfigureAwait(false);
                                        }
                                    }
                                }
                            }

                            lastLogId = logs.Max(obj => obj.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingService.AddServiceLogEntry(Data.Enumerations.General.LogEntryLevel.Warning, nameof(GuildRankingNotificationService), null, null, ex);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(30))
                              .ConfigureAwait(false);
                }
            }
        }

        #endregion // Methods

        #region SingletonLocatedServiceBase

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="serviceProvider">Service provider</param>
        /// <remarks>When this method is called all services are registered and can be resolved.  But not all singleton services may be initialized. </remarks>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task Initialize(IServiceProvider serviceProvider)
        {
            await base.Initialize(serviceProvider)
                      .ConfigureAwait(false);

            _discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();
            _discordClient.Connected += OnConnected;
        }

        #endregion // SingletonLocatedServiceBase
    }
}