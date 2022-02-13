using System.Collections.Concurrent;

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
/// Word chain game
/// </summary>
public class WordChainJob : LocatedAsyncJob
{
    #region Fields

    /// <summary>
    /// Words
    /// </summary>
    private static ConcurrentBag<string> _words = new ()
                                                  {
                                                      "Aleepeep",
                                                      "Anroov",
                                                      "Arooshaa",
                                                      "Baloop",
                                                      "Baroosh",
                                                      "Blarub",
                                                      "Bleedeep",
                                                      "Bleepdoop",
                                                      "Bloomanoo",
                                                      "Bloopdep",
                                                      "Blubla",
                                                      "Blubloop",
                                                      "Bludow",
                                                      "Bolroub",
                                                      "Boudip",
                                                      "Bullablopp",
                                                      "Bwilula",
                                                      "Childou",
                                                      "Clucop",
                                                      "Coddler",
                                                      "Cooroo",
                                                      "Daboof",
                                                      "Deiugoo",
                                                      "Doobroosh",
                                                      "Doodaar",
                                                      "Doolsileep",
                                                      "Doudoop",
                                                      "Drenikroovah",
                                                      "Drippadurp",
                                                      "Duidda",
                                                      "Eegyoo",
                                                      "Eereesh",
                                                      "Foipah",
                                                      "Fralloo",
                                                      "Fuanoo",
                                                      "Giplow",
                                                      "Gleeshaa",
                                                      "Gloop",
                                                      "Glubblug",
                                                      "Gommtoo",
                                                      "Grugrugoo",
                                                      "Gwoppip",
                                                      "Halooha",
                                                      "Harvester",
                                                      "Ishoonoo",
                                                      "Joohan",
                                                      "Jubjup",
                                                      "Juudow",
                                                      "Kerlapp",
                                                      "Khoofallaloo",
                                                      "Killeekee",
                                                      "Kreesha",
                                                      "Kukumuru",
                                                      "Kuumou",
                                                      "Larooo",
                                                      "Laulaua",
                                                      "Leemoola",
                                                      "Loilesh",
                                                      "Looah",
                                                      "Loojwah",
                                                      "Lookout",
                                                      "Luloovoru",
                                                      "Meeloomopp",
                                                      "Mikalooki",
                                                      "Miknoo",
                                                      "Moofrumaloo",
                                                      "Munloo",
                                                      "Muntap",
                                                      "Nautila",
                                                      "Neekoolaa",
                                                      "Nonwobb",
                                                      "Ooogh",
                                                      "Ooolpon",
                                                      "Ooonuuu",
                                                      "Oopdoop",
                                                      "Ooshanu",
                                                      "Panoowa",
                                                      "Peneloopee",
                                                      "Plipdoolb",
                                                      "Ploosi",
                                                      "Plunckdu",
                                                      "Polinque",
                                                      "Polliduup",
                                                      "Poobadoo",
                                                      "Poukounah",
                                                      "Protector",
                                                      "Puramatoo",
                                                      "Pwindwin",
                                                      "Qualdup",
                                                      "Queldip",
                                                      "Romperoo",
                                                      "Rosshelpa",
                                                      "Rubblubb",
                                                      "Rushioo",
                                                      "Shaashagugg",
                                                      "Sharoona",
                                                      "Shashoo",
                                                      "Shellguard",
                                                      "Shleroooah",
                                                      "Shoobaloosh",
                                                      "Shumooroovah",
                                                      "Sildroomp",
                                                      "Slashink",
                                                      "Slishaa",
                                                      "Sloolap",
                                                      "Slooshoo",
                                                      "Slupsloop",
                                                      "Soggumaa",
                                                      "Soumurrasou",
                                                      "Spelugg",
                                                      "Suwash",
                                                      "Suzoo",
                                                      "Swindau",
                                                      "Swooshaa",
                                                      "Swuulow",
                                                      "Tad",
                                                      "Tadpole",
                                                      "Talooboo",
                                                      "Thoorne",
                                                      "Tootoo",
                                                      "Toowal",
                                                      "Torblip",
                                                      "Wandill",
                                                      "Warthlop",
                                                      "Watcher",
                                                      "Willoo",
                                                      "Woomulla",
                                                      "Woplup",
                                                      "Worshipper",
                                                      "Wuaruakoo",
                                                      "Zoofioo"
                                                  };

    #endregion // Fields

    #region LocatedAsyncJob

    /// <summary>
    /// Executes the job
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task ExecuteOverrideAsync()
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var serviceProvider = Core.ServiceProviderContainer.Current.GetServiceProvider();
            await using (serviceProvider.ConfigureAwait(false))
            {
                var client = serviceProvider.GetService<DiscordSocketClient>();

                foreach (var gameChannel in dbFactory.GetRepository<GameChannelRepository>()
                                                     .GetQuery()
                                                     .Where(obj => obj.Type == GameType.WordChain)
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

                        if (messages.Count == 15
                         && messages.Any(obj => obj.Author.Id == client.CurrentUser.Id) == false
                         && messages[0].Content?.All(char.IsLetter) == true)
                        {
                            var lastLetter = messages[0].Content.Last();

                            var words = _words.Where(obj => obj.StartsWith(lastLetter.ToString(), StringComparison.InvariantCultureIgnoreCase))
                                              .ToList();

                            if (words.Count > 0)
                            {
                                await messageChannel.SendMessageAsync(words[new Random(DateTime.Now.Millisecond).Next(0, words.Count - 1)])
                                                    .ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
        }
    }

    #endregion // LocatedAsyncJob
}