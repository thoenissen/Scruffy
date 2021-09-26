using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Scruffy.Data.Json.ThatShaman;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord.Attributes;
using Scruffy.Services.GuildWars2;
using Scruffy.Services.WebApi;

namespace Scruffy.Commands
{
    /// <summary>
    /// Calendar commands
    /// </summary>
    [Group("gw2")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
    public class GuildWars2CommandBuilder : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public GuildWars2CommandBuilder(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Quaggan service
        /// </summary>
        public QuagganService QuagganService { get; set; }

        #endregion // Properties

        #region Command methods

        /// <summary>
        /// Creation of a one time reminder
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("quaggan")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
        public Task Quaggan(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await QuagganService.PostRandomQuaggan(commandContextContainer)
                                                       .ConfigureAwait(false);
                               });
        }

        /// <summary>
        /// Next update
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("update")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Standard)]
        public Task Update(CommandContext commandContext)
        {
            return InvokeAsync(commandContext,
                               async commandContextContainer =>
                               {
                                   await using (var connector = new ThatShamanConnector())
                                   {
                                       var now = DateTime.Now;

                                       var builder = new DiscordEmbedBuilder()
                                                        .WithThumbnail("https://cdn.discordapp.com/attachments/847555191842537552/861182143987712010/gw2.png")
                                                        .WithTitle(LocalizationGroup.GetText("GuildWars2Updates", "Guild Wars 2 - Updates"))
                                                        .WithColor(DiscordColor.Green)
                                                        .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/ef1f3e1f3f40100fb3750f8d7d25c657.png?size=64")
                                                        .WithTimestamp(now);

                                       void AddField(string fieldName, NextUpdateData data)
                                       {
                                           var field = new StringBuilder();
                                           field.Append("> ");
                                           field.Append(LocalizationGroup.GetText("Release", "Release"));
                                           field.Append(": ");
                                           field.Append(Formatter.MaskedUrl(data.When.ToLocalTime().ToString("G", LocalizationGroup.CultureInfo), new Uri("https://thatshaman.com/tools/countdown")));
                                           field.Append(Environment.NewLine);

                                           var timeSpan = data.When.ToLocalTime() - now;
                                           if (timeSpan > TimeSpan.Zero)
                                           {
                                               field.Append("> ");
                                               field.Append(LocalizationGroup.GetText("Timespan", "Timespan"));
                                               field.Append(": ");

                                               if (timeSpan.Days > 0)
                                               {
                                                   field.Append(timeSpan.Days);
                                                   field.Append(' ');
                                                   field.Append(timeSpan.Days == 1 ? LocalizationGroup.GetText("Day", "Day") : LocalizationGroup.GetText("Days", "Days"));
                                                   field.Append(' ');
                                               }

                                               if (timeSpan.TotalHours > 0)
                                               {
                                                   field.Append(timeSpan.Hours);
                                                   field.Append(' ');
                                                   field.Append(timeSpan.Hours == 1 ? LocalizationGroup.GetText("Hour", "Hour") : LocalizationGroup.GetText("Hours", "Hours"));
                                                   field.Append(' ');
                                               }

                                               field.Append(timeSpan.Minutes);
                                               field.Append(' ');
                                               field.Append(timeSpan.Minutes == 1 ? LocalizationGroup.GetText("Minute", "Minute") : LocalizationGroup.GetText("Minutes", "Minutes"));
                                               field.Append(' ');

                                               field.Append(timeSpan.Seconds);
                                               field.Append(' ');
                                               field.Append(timeSpan.Seconds == 1 ? LocalizationGroup.GetText("Second", "Second") : LocalizationGroup.GetText("Seconds", "Seconds"));
                                               field.Append(' ');

                                               field.Append('\n');
                                           }

                                           field.Append("> ");
                                           field.Append(LocalizationGroup.GetText("Confirmed", "Confirmed"));
                                           field.Append(": ");
                                           field.Append(data.Confirmed ? DiscordEmojiService.GetCheckEmoji(commandContextContainer.Client) : DiscordEmojiService.GetCrossEmoji(commandContextContainer.Client));

                                           if (data.Urls?.Count > 0)
                                           {
                                               var counter = 0;

                                               field.Append("\u200b (");

                                               if (data.Urls.Count == 1)
                                               {
                                                   field.Append(Formatter.MaskedUrl(LocalizationGroup.GetText("Source", "Source"), new Uri(data.Urls.First())));
                                               }
                                               else
                                               {
                                                   field.Append(LocalizationGroup.GetText("Sources", "Sources"));
                                                   field.Append(": ");
                                                   field.Append(string.Join(',', data.Urls.Select(obj => Formatter.MaskedUrl((++counter).ToString(), new Uri(obj)))));
                                               }

                                               field.Append(')');
                                           }

                                           field.Append(Environment.NewLine);
                                           field.Append('​');
                                           field.Append(Environment.NewLine);

                                           builder.AddField(fieldName, field.ToString());
                                       }

                                       AddField(LocalizationGroup.GetText("UpdateField", "General"),
                                                await connector.GetNextUpdate()
                                                               .ConfigureAwait(false));

                                       AddField(LocalizationGroup.GetText("EodField", "End of Dragons"),
                                                await connector.GetEODRelease()
                                                               .ConfigureAwait(false));

                                       await commandContextContainer.Message
                                                                    .RespondAsync(builder)
                                                                    .ConfigureAwait(false);
                                   }
                               });
        }

        #endregion // Command methods
    }
}
