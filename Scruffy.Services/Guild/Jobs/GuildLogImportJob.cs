using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Tables.Guild;
using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Enumerations.Guild;
using Scruffy.Data.Json.GuildWars2.Guild;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Guild.Jobs;

/// <summary>
/// Import guild log
/// </summary>
public class GuildLogImportJob : LocatedAsyncJob
{
    #region LocatedAsyncJob

    /// <inheritdoc/>
    public override async Task ExecuteOverrideAsync()
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var serviceProvider = ServiceProviderContainer.Current.GetServiceProvider();

            await using (serviceProvider.ConfigureAwait(false))
            {
                var discordClient = serviceProvider.GetService<DiscordSocketClient>();
                var guildRankService = new Lazy<GuildRankService>(() => serviceProvider.GetService<GuildRankService>());

                var channels = dbFactory.GetRepository<GuildChannelConfigurationRepository>()
                                        .GetQuery()
                                        .Select(obj => obj);

                foreach (var guild in dbFactory.GetRepository<GuildRepository>()
                                               .GetQuery()
                                               .Select(obj => new
                                               {
                                                   obj.Id,
                                                   obj.ApiKey,
                                                   obj.GuildId,
                                                   LastLogEntryId = obj.GuildLogEntries
                                                                                      .Select(obj2 => obj2.Id)
                                                                                      .OrderByDescending(obj2 => obj2)
                                                                                      .FirstOrDefault(),
                                                   ChannelId = channels.Where(obj2 => obj2.GuildId == obj.Id
                                                                                   && obj2.Type == GuildChannelConfigurationType.GuildLogNotification)
                                                                                      .Select(obj2 => (ulong?)obj2.DiscordChannelId)
                                                                                      .FirstOrDefault()
                                               })
                                               .ToList())
                {
                    var discordChannel = guild.ChannelId != null
                                             ? await discordClient.GetChannelAsync(guild.ChannelId.Value)
                                                                  .ConfigureAwait(false) as IMessageChannel
                                             : null;

                    var connector = new GuildWars2ApiConnector(guild.ApiKey);

                    await using (connector.ConfigureAwait(false))
                    {
                        try
                        {
                            foreach (var entry in (await connector.GetGuildLogEntries(guild.GuildId, guild.LastLogEntryId).ConfigureAwait(false)).OrderBy(obj => obj.Id))
                            {
                                if (dbFactory.GetRepository<GuildLogEntryRepository>()
                                             .Add(new GuildLogEntryEntity
                                                  {
                                                      GuildId = guild.Id,
                                                      Id = entry.Id,
                                                      Time = entry.Time.ToLocalTime(),
                                                      Type = entry.Type,
                                                      User = entry.User,
                                                      KickedBy = entry.KickedBy,
                                                      InvitedBy = entry.InvitedBy,
                                                      Operation = entry.Operation,
                                                      ItemId = entry.ItemId,
                                                      Count = entry.Count,
                                                      Coins = entry.Coins,
                                                      ChangedBy = entry.ChangedBy,
                                                      OldRank = entry.OldRank,
                                                      NewRank = entry.NewRank,
                                                      UpgradeId = entry.UpgradeId,
                                                      RecipeId = entry.RecipeId,
                                                      Action = entry.Action,
                                                      Activity = entry.Activity,
                                                      TotalParticipants = entry.TotalParticipants,
                                                      Participants = string.Join(';', entry.Participants?.Where(obj => string.IsNullOrWhiteSpace(obj) == false) ?? Array.Empty<string>()),
                                                      MessageOfTheDay = entry.MessageOfTheDay
                                                  }))
                                {
                                    var isProcessed = false;

                                    try
                                    {
                                        switch (entry.Type)
                                        {
                                            case GuildLogEntryEntity.Types.Joined:
                                                {
                                                    isProcessed = await OnJoined(discordChannel, entry).ConfigureAwait(false);
                                                }
                                                break;

                                            case GuildLogEntryEntity.Types.Kick:
                                                {
                                                    isProcessed = await OnKick(discordChannel, entry).ConfigureAwait(false);
                                                }
                                                break;

                                            case GuildLogEntryEntity.Types.RankChange:
                                                {
                                                    isProcessed = await OnRankChanged(discordChannel, guildRankService.Value, guild.Id, entry).ConfigureAwait(false);
                                                }
                                                break;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LoggingService.AddJobLogEntry(LogEntryLevel.Error, nameof(GuildLogImportJob), ex.Message, null, ex);
                                    }

                                    if (isProcessed)
                                    {
                                        dbFactory.GetRepository<GuildLogEntryRepository>()
                                                 .Refresh(obj => obj.GuildId == guild.Id
                                                                 && obj.Id == entry.Id,
                                                          obj => obj.IsProcessed = true);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggingService.AddJobLogEntry(LogEntryLevel.Warning, nameof(GuildLogImportJob), ex.Message, null, ex);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Member joined
    /// </summary>
    /// <param name="discordChannel">Discord channel</param>
    /// <param name="entry">Entry</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task<bool> OnJoined(IMessageChannel discordChannel, GuildLogEntry entry)
    {
        if (discordChannel != null)
        {
            await discordChannel.SendMessageAsync(LocalizationGroup.GetFormattedText("MemberJoined", "**{0}** joined the guild.", entry.User))
                                .ConfigureAwait(false);
        }

        return true;
    }

    /// <summary>
    /// Member kicked
    /// </summary>
    /// <param name="discordChannel">Discord channel</param>
    /// <param name="entry">Entry</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task<bool> OnKick(IMessageChannel discordChannel, GuildLogEntry entry)
    {
        if (discordChannel != null)
        {
            if (entry.User == entry.KickedBy)
            {
                await discordChannel.SendMessageAsync(LocalizationGroup.GetFormattedText("MemberLeft", "**{0}** left the guild.", entry.User))
                                    .ConfigureAwait(false);
            }
            else
            {
                await discordChannel.SendMessageAsync(LocalizationGroup.GetFormattedText("MemberKicked", "**{0}** got kicked out of the guild by {1}.", entry.User, entry.KickedBy))
                                    .ConfigureAwait(false);
            }
        }

        return true;
    }

    /// <summary>
    /// Rank changed
    /// </summary>
    /// <param name="discordChannel">Discord channel</param>
    /// <param name="guildRankService">Guild rank service</param>
    /// <param name="guildId">Id of the guild</param>
    /// <param name="entry">Entry</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task<bool> OnRankChanged(IMessageChannel discordChannel, GuildRankService guildRankService, long guildId, GuildLogEntry entry)
    {
        var refreshTask = guildRankService.RefreshDiscordRank(guildId, entry.User, entry.NewRank);

        if (discordChannel != null)
        {
            await discordChannel.SendMessageAsync(LocalizationGroup.GetFormattedText("MemberRankChanged", "**{0}** changed the rank of **{1}** from **{2}** to **{3}**.", entry.ChangedBy, entry.User, entry.OldRank, entry.NewRank))
                                .ConfigureAwait(false);
        }

        await refreshTask.ConfigureAwait(false);

        return true;
    }

    #endregion // LocatedAsyncJob
}