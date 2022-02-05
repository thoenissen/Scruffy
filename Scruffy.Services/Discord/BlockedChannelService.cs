using System.Collections.Concurrent;

using Discord;
using Discord.Commands;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Tables.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Discord;

/// <summary>
/// Blocked discord channel management
/// </summary>
public class BlockedChannelService : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Blocked channels
    /// </summary>
    private readonly ConcurrentDictionary<(ulong Server, ulong ChannelId), byte> _blockedChannels;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public BlockedChannelService(LocalizationService localizationService)
        : base(localizationService)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var channels = dbFactory.GetRepository<BlockedDiscordChannelRepository>()
                                    .GetQuery()
                                    .ToList();

            _blockedChannels = new ConcurrentDictionary<(ulong Server, ulong ChannelId), byte>(channels.ToDictionary(obj => (obj.ServerId, obj.ChannelId), obj => (byte)0));
        }
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Check if the channel ist blocked
    /// </summary>
    /// <param name="commandContext">Context</param>
    /// <returns>Is channel blocked?</returns>
    public bool IsChannelBlocked(ICommandContext commandContext)
    {
        var isBlocked = commandContext.Channel is IGuildChannel guildChannel
                     && _blockedChannels.ContainsKey((guildChannel.GuildId, guildChannel.Id));

        if (isBlocked)
        {
            commandContext.Message.ReplyAsync(LocalizationGroup.GetText("ChannelIsBlocked", "You can't use this command here. Please use the designated channels."));
        }

        return isBlocked;
    }

    /// <summary>
    /// Adding a channel
    /// </summary>
    /// <param name="channel">Channel</param>
    /// <returns>Could the channel be added?</returns>
    public bool AddChannel(IGuildChannel channel)
    {
        var success = false;

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            success = dbFactory.GetRepository<BlockedDiscordChannelRepository>()
                               .Add(new BlockedDiscordChannelEntity
                                    {
                                        ServerId = channel.GuildId,
                                        ChannelId = channel.Id
                                    });

            if (success)
            {
                _blockedChannels.TryAdd((channel.GuildId, channel.Id), 0);
            }
        }

        return success;
    }

    /// <summary>
    /// Removing a channel
    /// </summary>
    /// <param name="channel">Channel</param>
    /// <returns>Could the channel be removed?</returns>
    public bool RemoveChannel(IGuildChannel channel)
    {
        var success = false;

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            success = dbFactory.GetRepository<BlockedDiscordChannelRepository>()
                               .Remove(obj => obj.ServerId == channel.GuildId
                                           && obj.ChannelId == channel.Id);

            if (success)
            {
                _blockedChannels.TryRemove((channel.GuildId, channel.Id), out var _);
            }
        }

        return success;
    }

    #endregion // Methods
}