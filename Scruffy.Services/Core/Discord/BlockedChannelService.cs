using System.Collections.Concurrent;

using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Tables.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Core.Discord
{
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
        /// <param name="channel">Channel</param>
        /// <returns>Is channel blocked?</returns>
        public bool IsChannelBlocked(CommandContext commandContext, DiscordChannel channel)
        {
            var isBlocked = channel?.GuildId != null
                         && _blockedChannels.ContainsKey((channel.GuildId.Value, channel.Id));

            if (isBlocked)
            {
                commandContext.Message.RespondAsync(LocalizationGroup.GetText("ChannelIsBlocked", "You can't use this command here. Please use the designated channels."));
            }

            return isBlocked;
        }

        /// <summary>
        /// Adding a channel
        /// </summary>
        /// <param name="channel">Channel</param>
        /// <returns>Could the channel be added?</returns>
        public bool AddChannel(DiscordChannel channel)
        {
            var success = false;

            if (channel.GuildId != null)
            {
                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    success = dbFactory.GetRepository<BlockedDiscordChannelRepository>()
                                       .Add(new BlockedDiscordChannelEntity
                                            {
                                                ServerId = channel.GuildId.Value,
                                                ChannelId = channel.Id
                                            });

                    if (success)
                    {
                        _blockedChannels.TryAdd((channel.GuildId.Value, channel.Id), 0);
                    }
                }
            }

            return success;
        }

        /// <summary>
        /// Removing a channel
        /// </summary>
        /// <param name="channel">Channel</param>
        /// <returns>Could the channel be removed?</returns>
        public bool RemoveChannel(DiscordChannel channel)
        {
            var success = false;

            if (channel.GuildId != null)
            {
                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    success = dbFactory.GetRepository<BlockedDiscordChannelRepository>()
                                       .Remove(obj => obj.ServerId == channel.GuildId.Value
                                                   && obj.ChannelId == channel.Id);

                    if (success)
                    {
                        _blockedChannels.TryRemove((channel.GuildId.Value, channel.Id), out var _);
                    }
                }
            }

            return success;
        }

        #endregion // Methods
    }
}
