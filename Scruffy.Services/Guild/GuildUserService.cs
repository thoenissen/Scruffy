using System.Collections.Concurrent;

using Discord;
using Discord.WebSocket;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Enumerations.Guild;
using Scruffy.Data.Services.Guild;
using Scruffy.Services.Core;

namespace Scruffy.Services.Guild;

/// <summary>
/// Guild user service
/// </summary>
public sealed class GuildUserService : SingletonLocatedServiceBase, IDisposable
{
    #region Fields

    /// <summary>
    /// Notification channels
    /// </summary>
    private ConcurrentDictionary<ulong, ulong> _notificationChannels;

    /// <summary>
    /// Welcome messages
    /// </summary>
    private ConcurrentDictionary<ulong, WelcomeDirectMessageData> _welcomeMessages;

    /// <summary>
    /// Role of new users
    /// </summary>
    private ConcurrentDictionary<ulong, ulong> _newUserRoles;

    /// <summary>
    /// Discord client
    /// </summary>
    private DiscordSocketClient _discordClient;

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Set channel
    /// </summary>
    /// <param name="serverId">Server id</param>
    /// <param name="channelId">Channel id</param>
    public void SetChannel(ulong serverId, ulong channelId)
    {
        _notificationChannels.AddOrUpdate(serverId, channelId, (o, n) => channelId);

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var guildId = dbFactory.GetRepository<GuildRepository>()
                                   .GetQuery()
                                   .Where(obj => obj.DiscordServerId == serverId)
                                   .Select(obj => obj.Id)
                                   .FirstOrDefault();

            if (guildId > 0)
            {
                dbFactory.GetRepository<GuildChannelConfigurationRepository>()
                         .AddOrRefresh(obj => obj.GuildId == guildId
                                           && obj.Type ==  GuildChannelConfigurationType.UserNotification,
                                       obj =>
                                       {
                                           obj.GuildId = guildId;
                                           obj.Type = GuildChannelConfigurationType.UserNotification;
                                           obj.DiscordChannelId = channelId;
                                       });
            }
        }
    }

    /// <summary>
    /// User unbanned
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="guild">Guild</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private Task OnUserUnbanned(SocketUser user, SocketGuild guild) => SendNotificationMessage(user, guild, "UserUnbanned");

    /// <summary>
    /// User banned
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="guild">Guild</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private Task OnUserBanned(SocketUser user, SocketGuild guild) => SendNotificationMessage(user, guild, "UserBanned");

    /// <summary>
    /// User left
    /// </summary>
    /// <param name="guild">Guild</param>
    /// <param name="user">User</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private Task OnUserLeft(SocketGuild guild, SocketUser user) => SendNotificationMessage(user, guild, "UserLeft");

    /// <summary>
    /// User joined
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OnUserJoined(SocketGuildUser user)
    {
        await SetNewUserRole(user, user.Guild).ConfigureAwait(false);

        await SendNotificationMessage(user, user.Guild, "UserJoined").ConfigureAwait(false);

        await SendWelcomeMessage(user, user.Guild).ConfigureAwait(false);
    }

    /// <summary>
    /// Set new user role
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="guild">Guild</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task SetNewUserRole(SocketGuildUser user, SocketGuild guild)
    {
        try
        {
            if (_newUserRoles.TryGetValue(guild.Id, out var roleId))
            {
                await user.AddRoleAsync(roleId)
                          .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(GuildUserService), "Set new user role failed", null, ex);
        }
    }

    /// <summary>
    /// Send notification
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="guild">Guild</param>
    /// <param name="messageKey">Message key</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task SendNotificationMessage(SocketUser user, SocketGuild guild, string messageKey)
    {
        try
        {
            if (_notificationChannels.TryGetValue(guild.Id, out var channelId))
            {
                var userName = $"{user.Username}#{user.Discriminator}";

                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    var discordAccounts = dbFactory.GetRepository<DiscordAccountRepository>()
                                                   .GetQuery()
                                                   .Select(obj => obj);

                    var guildWarsAccounts = await dbFactory.GetRepository<GuildWarsAccountRepository>()
                                                           .GetQuery()
                                                           .Where(obj => discordAccounts.Any(obj2 => obj2.UserId == obj.UserId
                                                                                                  && obj2.Id == user.Id))
                                                           .Select(obj => obj.Name)
                                                           .ToListAsync()
                                                           .ConfigureAwait(false);

                    if (guildWarsAccounts.Count > 0)
                    {
                        userName = $"{userName} ({string.Join(", ", guildWarsAccounts)})";
                    }
                }

                var channel = await _discordClient.GetChannelAsync(channelId)
                                                  .ConfigureAwait(false);
                if (channel is ITextChannel textChannel)
                {
                    await textChannel.SendMessageAsync(LocalizationGroup.GetFormattedText(messageKey, messageKey + " {0}", userName))
                                     .ConfigureAwait(false);
                }
                else
                {
                    LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(GuildUserService), "Invalid notification channel", guild.Id + "-" + channelId);
                }
            }
        }
        catch (Exception ex)
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(GuildUserService), "Notification failed", null, ex);
        }
    }

    /// <summary>
    /// Send welcome direct message
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="guild">Guild</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task SendWelcomeMessage(SocketGuildUser user, SocketGuild guild)
    {
        try
        {
            if (_welcomeMessages.TryGetValue(guild.Id, out var data))
            {
                LoggingService.AddServiceLogEntry(LogEntryLevel.Information, nameof(GuildUserService), "Sending welcome message", guild.Id + "-" + user.Id);

                var builder = new EmbedBuilder().WithColor(Color.Green)
                                                .WithTimestamp(DateTime.Now);

                if (string.IsNullOrWhiteSpace(data.Title) == false)
                {
                    builder.WithTitle(data.Title);
                }

                if (string.IsNullOrWhiteSpace(data.Description) == false)
                {
                    builder.WithDescription(data.Description.Replace("{{USER}}", user.Mention));
                }

                if (data.Fields?.Count > 0)
                {
                    foreach (var (title, text) in data.Fields)
                    {
                        builder.AddField(title, text);
                    }
                }

                if (string.IsNullOrWhiteSpace(data.Footer) == false)
                {
                    builder.AddField("\u200b", data.Footer);
                }

                var channel = await user.CreateDMChannelAsync()
                                        .ConfigureAwait(false);

                await channel.SendMessageAsync(embed: builder.Build())
                             .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(GuildUserService), "Welcome message failed", null, ex);
        }
    }

    #endregion // Methods

    #region SingletonLocatedServiceBase

    /// <inheritdoc/>
    public override async Task Initialize(IServiceProvider serviceProvider)
    {
        await base.Initialize(serviceProvider)
                  .ConfigureAwait(false);

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            _notificationChannels = new ConcurrentDictionary<ulong, ulong>(dbFactory.GetRepository<GuildChannelConfigurationRepository>()
                                                                                    .GetQuery()
                                                                                    .Where(obj => obj.Type == GuildChannelConfigurationType.UserNotification)
                                                                                    .Select(obj => new
                                                                                                   {
                                                                                                       obj.Guild.DiscordServerId,
                                                                                                       obj.DiscordChannelId
                                                                                                   })
                                                                                    .ToDictionary(obj => obj.DiscordServerId, obj => obj.DiscordChannelId));

            _welcomeMessages = new ConcurrentDictionary<ulong, WelcomeDirectMessageData>(dbFactory.GetRepository<GuildRepository>()
                                                                                                  .GetQuery()
                                                                                                  .Where(obj => obj.WelcomeDirectMessage != null)
                                                                                                  .Select(obj => new
                                                                                                                 {
                                                                                                                     obj.DiscordServerId,
                                                                                                                     obj.WelcomeDirectMessage
                                                                                                                 })
                                                                                                  .AsEnumerable()
                                                                                                  .ToDictionary(obj => obj.DiscordServerId, obj => JsonConvert.DeserializeObject<WelcomeDirectMessageData>(obj.WelcomeDirectMessage)));

            _newUserRoles = new ConcurrentDictionary<ulong, ulong>(dbFactory.GetRepository<GuildRepository>()
                                                                            .GetQuery()
                                                                            .Where(obj => obj.NewUserDiscordRoleId != null)
                                                                            .Select(obj => new
                                                                                           {
                                                                                               obj.DiscordServerId,
                                                                                               RoleId = obj.NewUserDiscordRoleId ?? 0
                                                                                           })
                                                                            .ToDictionary(obj => obj.DiscordServerId, obj => obj.RoleId));
        }

        _discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();
        _discordClient.UserJoined += OnUserJoined;
        _discordClient.UserLeft += OnUserLeft;
        _discordClient.UserBanned += OnUserBanned;
        _discordClient.UserUnbanned += OnUserUnbanned;
    }

    #endregion // SingletonLocatedServiceBase

    #region IDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        if (_discordClient != null)
        {
            _discordClient.UserJoined -= OnUserJoined;
            _discordClient.UserLeft -= OnUserLeft;
            _discordClient.UserBanned -= OnUserBanned;
            _discordClient.UserUnbanned -= OnUserUnbanned;
            _discordClient = null;
        }
    }

    #endregion // IDisposable
}