
using Discord;
using Discord.WebSocket;

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
            var client = serviceProvider.GetService<DiscordSocketClient>();

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

                foreach (var guild in client.Guilds)
                {
                    foreach (var channel in guild.Channels
                                                 .OfType<ITextChannel>())
                    {
                        var lastImport = lastImports.FirstOrDefault(obj => obj.ServerId == guild.Id
                                                                        && obj.ChannelId == channel.Id);

                        var importLimit = lastImport?.TimeStamp ?? maximumImportLimit;

                        var messages = new List<IMessage>();

                        await foreach (var collection in channel.GetMessagesAsync(1).ConfigureAwait(false))
                        {
                            messages.AddRange(collection);
                        }

                        while (messages?.Count > 0)
                        {
                            foreach (var message in messages.OfType<IUserMessage>()
                                                            .Where(obj => obj.Author.IsBot == false))
                            {
                                if (message.Timestamp.LocalDateTime > importLimit)
                                {
                                    importData.Add(new DiscordMessageBulkInsertData
                                                   {
                                                       ServerId = guild.Id,
                                                       ChannelId = channel.Id,
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
                                messages = new List<IMessage>();

                                await foreach (var collection in channel.GetMessagesAsync(messages[^1], Direction.Before).ConfigureAwait(false))
                                {
                                    messages.AddRange(collection);
                                }
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