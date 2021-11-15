﻿using DSharpPlus;
using DSharpPlus.Entities;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildAdministration;
using Scruffy.Data.Entity.Tables.Guild;
using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Enumerations.GuildAdministration;
using Scruffy.Data.Json.GuildWars2.Guild;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.GuildAdministration.Jobs;

/// <summary>
/// Import guild log
/// </summary>
public class GuildLogImportJob : LocatedAsyncJob
{
    #region LocatedAsyncJob

    /// <summary>
    /// Executes the job
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task ExecuteAsync()
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider();
            await using (serviceProvider.ConfigureAwait(false))
            {
                var discordClient = serviceProvider.GetService<DiscordClient>();

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
                                                                  .ConfigureAwait(false)
                                             : null;

                    var connector = new GuidWars2ApiConnector(guild.ApiKey);
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
                                    switch (entry.Type)
                                    {
                                        case GuildLogEntryEntity.Types.Joined:
                                            {
                                                await OnJoined(discordChannel, entry).ConfigureAwait(false);
                                            }
                                            break;

                                        case GuildLogEntryEntity.Types.Kick:
                                            {
                                                await OnKick(discordChannel, entry).ConfigureAwait(false);
                                            }
                                            break;
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
    private async Task OnJoined(DiscordChannel discordChannel, GuildLogEntry entry)
    {
        if (discordChannel != null)
        {
            await discordChannel.SendMessageAsync(LocalizationGroup.GetFormattedText("MemberJoined", "**{0}** joined the guild.", entry.User))
                                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Member kicked
    /// </summary>
    /// <param name="discordChannel">Discord channel</param>
    /// <param name="entry">Entry</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OnKick(DiscordChannel discordChannel, GuildLogEntry entry)
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
    }

    #endregion // LocatedAsyncJob
}