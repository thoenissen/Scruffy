using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Games;
using Scruffy.Data.Enumerations.Games;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;

namespace Scruffy.Services.Games.Jobs;

/// <summary>
/// Counter game
/// </summary>
public class CounterGameJob : LocatedAsyncJob
{
    #region LocatedAsyncJob

    /// <summary>
    /// Executes the job
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task ExecuteOverrideAsync()
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var serviceProvider = ServiceProviderContainer.Current.GetServiceProvider();
            await using (serviceProvider.ConfigureAwait(false))
            {
                var client = serviceProvider.GetService<DiscordSocketClient>();

                foreach (var gameChannel in dbFactory.GetRepository<GameChannelRepository>()
                                                     .GetQuery()
                                                     .Where(obj => obj.Type == GameType.Counter)
                                                     .Select(obj => new
                                                                    {
                                                                        obj.DiscordChannelId
                                                                    })
                                                     .ToList())
                {
                    var channel = await client.GetChannelAsync(gameChannel.DiscordChannelId)
                                              .ConfigureAwait(false);

                    if (channel is IMessageChannel messageChannel)
                    {
                        var messages = new List<IMessage>();

                        await foreach (var collection in messageChannel.GetMessagesAsync(15).ConfigureAwait(false))
                        {
                            messages.AddRange(collection);
                        }

                        if (messages?.Count == 15
                         && messages.Any(obj => obj.Author.Id == client.CurrentUser.Id) == false
                         && messages[0].Id % 3 == 2
                         && messages[0].Content?.All(char.IsDigit) == true
                         && messages[1].Content?.All(char.IsDigit) == true
                         && int.TryParse(messages[0].Content, out var currentNumber)
                         && int.TryParse(messages[1].Content, out var lastNumber)
                         && currentNumber == lastNumber + 1)
                        {
                            await messageChannel.SendMessageAsync((++currentNumber).ToString())
                                                .ConfigureAwait(false);
                        }
                    }
                }
            }
        }
    }

    #endregion // LocatedAsyncJob
}