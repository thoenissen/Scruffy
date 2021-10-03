using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildAdministration;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.GuildAdministration
{
    /// <summary>
    /// Guild bank services
    /// </summary>
    public class GuildBankService : LocatedServiceBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public GuildBankService(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        /// <summary>
        /// Validation the guild bank
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Check(CommandContextContainer commandContext)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var guild = dbFactory.GetRepository<GuildRepository>()
                                      .GetQuery()
                                      .Where(obj => obj.DiscordServerId == commandContext.Guild.Id)
                                      .Select(obj => new
                                                     {
                                                         obj.ApiKey,
                                                         obj.GuildId
                                                     })
                                      .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(guild?.ApiKey) == false)
                {
                    var msg = new DiscordEmbedBuilder();
                    msg.WithTitle(LocalizationGroup.GetText("BankValidation", "Guild bank validation"));
                    msg.WithDescription(LocalizationGroup.GetText("BankValidationResult", "In the following message you can see the results of the bank validation."));
                    msg.WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/ef1f3e1f3f40100fb3750f8d7d25c657.png?size=64");
                    msg.WithTimestamp(DateTime.Now);

                    await using (var connector = new GuidWars2ApiConnector(guild.ApiKey))
                    {
                        var vault =  await connector.GetGuildVault(guild.GuildId)
                                                    .ConfigureAwait(false);

                        foreach (var stash in vault)
                        {
                            var slots = new List<(int ItemId, int X, int Y)>();
                            var i = 0;

                            foreach (var slot in stash.Slots)
                            {
                                if (slot != null
                                 && slot.Count < 250)
                                {
                                    slots.Add((slot.ItemId, (i % 10) + 1, (i / 10) + 1));
                                }

                                i++;
                            }

                            var stringBuilder = new StringBuilder();

                            foreach (var group in slots.ToLookup(obj => obj.ItemId, obj => (obj.X, obj.Y))
                                                       .Where(obj => obj.Count() > 1))
                            {
                                var item = await connector.GetItem(group.Key)
                                                          .ConfigureAwait(false);

                                stringBuilder.AppendLine(item.Name);

                                foreach (var (x, y) in group.OrderBy(obj => obj.X)
                                                            .ThenBy(obj => obj.Y))
                                {
                                    stringBuilder.AppendLine($" - " + Formatter.InlineCode($"({x}/{y})"));
                                }
                            }

                            if (stringBuilder.Length > 0)
                            {
                                stringBuilder.Insert(0, Formatter.Bold(LocalizationGroup.GetText("DuplicationCheck", "Duplication check")) + " \u200B " + DiscordEmojiService.GetCrossEmoji(commandContext.Client) + "\n");
                            }
                            else
                            {
                                stringBuilder.AppendLine(Formatter.Bold(LocalizationGroup.GetText("DuplicationCheck", "Duplication check")) + " \u200B " + DiscordEmojiService.GetCheckEmoji(commandContext.Client));
                            }

                            if (stash.Coins != 0 && stash.Coins % 10000 != 0)
                            {
                                var goldCoins = stash.Coins / 10000;
                                var silverCoins = (stash.Coins - (goldCoins * 10000)) / 100;
                                var copperCoins = stash.Coins % 100;

                                stringBuilder.AppendLine(Formatter.Bold(LocalizationGroup.GetText("CoinCheck", "Coins check")) + " \u200B " + DiscordEmojiService.GetCrossEmoji(commandContext.Client));
                                stringBuilder.AppendLine($"{goldCoins} {DiscordEmojiService.GetGuildWars2GoldEmoji(commandContext.Client)} {silverCoins} {DiscordEmojiService.GetGuildWars2SilverEmoji(commandContext.Client)} {copperCoins} {DiscordEmojiService.GetGuildWars2CopperEmoji(commandContext.Client)}");
                            }
                            else
                            {
                                stringBuilder.AppendLine(Formatter.Bold(LocalizationGroup.GetText("CoinCheck", "Coins check")) + " \u200B " + DiscordEmojiService.GetCheckEmoji(commandContext.Client));
                            }

                            stringBuilder.Append("\u200B");

                            msg.AddField(stash.Note ?? "Stash", stringBuilder.ToString());
                        }
                    }

                    await commandContext.Channel
                                        .SendMessageAsync(msg)
                                        .ConfigureAwait(false);
                }
                else
                {
                    await commandContext.Channel
                                        .SendMessageAsync(LocalizationGroup.GetText("NoApiKey", "The guild ist not configured."))
                                        .ConfigureAwait(false);
                }
            }
        }
    }
}
