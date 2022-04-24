using Discord;

using Scruffy.Data.Json.ThatShaman;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Interfaces;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.GuildWars2;

/// <summary>
/// Guild Wars 2 update service
/// </summary>
public class GuildWarsUpdateService : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// thatshaman.com connector
    /// </summary>
    private ThatShamanConnector _thatShamanConnector;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="thatShamanConnector">thatshaman.com connector</param>
    public GuildWarsUpdateService(LocalizationService localizationService, ThatShamanConnector thatShamanConnector)
        : base(localizationService)
    {
        _thatShamanConnector = thatShamanConnector;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Post update overview
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PostUpdateOverview(IContextContainer commandContext)
    {
        var now = DateTime.Now;

        var builder = new EmbedBuilder().WithThumbnailUrl("https://cdn.discordapp.com/attachments/847555191842537552/861182143987712010/gw2.png")
                                        .WithTitle(LocalizationGroup.GetText("GuildWars2Updates", "Guild Wars 2 - Updates"))
                                        .WithColor(Color.Green)
                                        .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                        .WithTimestamp(now);

        void AddField(string fieldName, NextUpdateData data)
        {
            var field = new StringBuilder();
            field.Append("> ");
            field.Append(LocalizationGroup.GetText("Release", "Release"));
            field.Append(": ");

            field.Append(Format.Url(data.When.ToLocalTime()
                                        .ToString("G", LocalizationGroup.CultureInfo),
                                    "https://thatshaman.com/tools/countdown"));
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

                    field.Append(timeSpan.Days == 1
                                     ? LocalizationGroup.GetText("Day", "Day")
                                     : LocalizationGroup.GetText("Days", "Days"));
                    field.Append(' ');
                }

                if (timeSpan.TotalHours > 0)
                {
                    field.Append(timeSpan.Hours);
                    field.Append(' ');

                    field.Append(timeSpan.Hours == 1
                                     ? LocalizationGroup.GetText("Hour", "Hour")
                                     : LocalizationGroup.GetText("Hours", "Hours"));
                    field.Append(' ');
                }

                field.Append(timeSpan.Minutes);
                field.Append(' ');

                field.Append(timeSpan.Minutes == 1
                                 ? LocalizationGroup.GetText("Minute", "Minute")
                                 : LocalizationGroup.GetText("Minutes", "Minutes"));
                field.Append(' ');

                field.Append(timeSpan.Seconds);
                field.Append(' ');

                field.Append(timeSpan.Seconds == 1
                                 ? LocalizationGroup.GetText("Second", "Second")
                                 : LocalizationGroup.GetText("Seconds", "Seconds"));
                field.Append(' ');

                field.Append('\n');
            }

            field.Append("> ");
            field.Append(LocalizationGroup.GetText("Confirmed", "Confirmed"));
            field.Append(": ");

            field.Append(data.Confirmed
                             ? DiscordEmoteService.GetCheckEmote(commandContext.Client)
                             : DiscordEmoteService.GetCrossEmote(commandContext.Client));

            if (data.Urls?.Count > 0)
            {
                var counter = 0;

                field.Append("\u200b (");

                if (data.Urls.Count == 1)
                {
                    field.Append(Format.Url(LocalizationGroup.GetText("Source", "Source"), data.Urls.First()));
                }
                else
                {
                    field.Append(LocalizationGroup.GetText("Sources", "Sources"));
                    field.Append(": ");
                    field.Append(string.Join(',', data.Urls.Select(obj => Format.Url((++counter).ToString(), obj))));
                }

                field.Append(')');
            }

            field.Append(Environment.NewLine);
            field.Append('​');
            field.Append(Environment.NewLine);

            builder.AddField(fieldName, field.ToString());
        }

        AddField(LocalizationGroup.GetText("UpdateField", "General"),
                 await _thatShamanConnector.GetNextUpdate()
                                           .ConfigureAwait(false));

        await commandContext.ReplyAsync(embed: builder.Build())
                            .ConfigureAwait(false);
    }

    #endregion // Methods
}