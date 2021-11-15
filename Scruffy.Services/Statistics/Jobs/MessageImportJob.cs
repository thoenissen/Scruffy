using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Statistics;
using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Services.Statistics;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Statistics.Jobs;

/// <summary>
/// Import discord messages
/// </summary>
public class MessageImportJob : LocatedAsyncJob
{
    #region LocatedAsyncJob

    /// <summary>
    /// Executes the job
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task ExecuteAsync()
    {
        var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider();
        await using (serviceProvider.ConfigureAwait(false))
        {
            var client = serviceProvider.GetService<DiscordClient>();

            var maximumImportLimit = DateTime.Now.AddDays(-60);

            var importData = new List<DiscordMessageBulkInsertData>();

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var lastImports = dbFactory.GetRepository<DiscordMessageRepository>()
                                           .GetQuery()
                                           .Where(obj => obj.IsBatchCommitted == true)
                                           .GroupBy(obj => new
                                                           {
                                                               ServerId = obj.DiscordServerId,
                                                               ChannelId = obj.DiscordChannelId
                                                           })
                                           .Select(obj => new
                                                          {
                                                              obj.Key.ServerId,
                                                              obj.Key.ChannelId,
                                                              TimeStamp = obj.Max(obj2 => obj2.TimeStamp)
                                                          })
                                           .ToList();

                foreach (var (guildId, guild) in client.Guilds)
                {
                    foreach (var (channelId, channel) in guild.Channels
                                                              .Where(obj => obj.Value.Type == ChannelType.Text))
                    {
                        var lastImport = lastImports.FirstOrDefault(obj => obj.ServerId == guildId
                                                                        && obj.ChannelId == channelId);

                        var importLimit = lastImport?.TimeStamp ?? maximumImportLimit;

                        var messages = await channel.GetMessagesAsync(1)
                                                    .ConfigureAwait(false);

                        while (messages?.Count > 0)
                        {
                            foreach (var message in messages.Where(obj => obj.Author.IsBot == false
                                                                       && obj.WebhookId == null))
                            {
                                if (message.Timestamp.LocalDateTime > importLimit)
                                {
                                    importData.Add(new DiscordMessageBulkInsertData
                                                   {
                                                       ServerId = guildId,
                                                       ChannelId = channelId,
                                                       UserId = message.Author.Id,
                                                       MessageId = message.Id,
                                                       TimeStamp = message.Timestamp.LocalDateTime
                                                   });
                                }
                                else
                                {
                                    messages = null;

                                    break;
                                }
                            }

                            if (messages != null)
                            {
                                messages = await channel.GetMessagesBeforeAsync(messages[^1].Id)
                                                        .ConfigureAwait(false);
                            }
                        }
                    }
                }

                if (await dbFactory.GetRepository<DiscordMessageRepository>()
                                   .BulkInsert(importData, true)
                                   .ConfigureAwait(false) == false)
                {
                    LoggingService.AddJobLogEntry(LogEntryLevel.Error, nameof(MessageImportJob), "BulkInsert", dbFactory.LastError?.Message, dbFactory.LastError?.ToString());
                }
            }
        }
    }

    #endregion // LocatedAsyncJob
}