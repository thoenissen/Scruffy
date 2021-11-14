using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Scruffy.Data.Json.ThatShaman;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.GuildWars2
{
    /// <summary>
    /// Guild Wars 2 update service
    /// </summary>
    public class GuildWarsUpdateService : LocatedServiceBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public GuildWarsUpdateService(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Post update overview
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PostUpdateOverview(CommandContextContainer commandContext)
        {
            var connector = new ThatShamanConnector();
            await using (connector.ConfigureAwait(false))
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
                    field.Append(data.Confirmed ? DiscordEmojiService.GetCheckEmoji(commandContext.Client) : DiscordEmojiService.GetCrossEmoji(commandContext.Client));

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

                await commandContext.Message
                                    .RespondAsync(builder)
                                    .ConfigureAwait(false);
            }
        }

        #endregion // Methods
    }
}
