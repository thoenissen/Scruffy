using Discord;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord.Interfaces;

namespace Scruffy.Services.Information;

/// <summary>
/// Handling information commands
/// </summary>
public class InformationCommandHandler : LocatedServiceBase
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="InformationCommandHandler"/> class.
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public InformationCommandHandler(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Shows general information about Scruffy
    /// </summary>
    /// <param name="context">Context container</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task Info(IContextContainer context)
    {
        var builder = new EmbedBuilder()
                      .WithColor(Color.Green)
                      .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                      .WithTimestamp(DateTime.Now)
                      .WithTitle(LocalizationGroup.GetText("GeneralInformationTitle", "General information"))
                      .WithDescription(LocalizationGroup.GetFormattedText("GeneralInformationDescription", "All data collected by {0} is used only for the administration of the guild. The individual areas where data is collected are discussed below.", context.Client.CurrentUser.Mention))
                      .AddField(LocalizationGroup.GetText("GuildWars2ApiTitle", "Guild Wars 2 - API-Token"), LocalizationGroup.GetFormattedText("GuildWars2ApiDescription", "The following data is determined via the specified API token:\n\n - With how many characters is the guild represented?\n - When was the last login?\n - Which server is the account assigned to?\n\nAll other data is only determined temporarily when you request {0} to execute a command. You only ever have access to your own API token.", context.Client.CurrentUser.Mention))
                      .AddField(LocalizationGroup.GetText("DiscordTitle", "Discord"), LocalizationGroup.GetFormattedText("DiscordDescription", "Part of the guild ranking system is the activity in Discord. For this, how many messages are written and how many minutes you are in the voice chat are saved. The content of the posts is irrelevant and is not saved.", context.Client.CurrentUser.Mention))
                      .AddField(LocalizationGroup.GetText("ExtendedGuildStatisticsTitle", "Extended guild statistics"), LocalizationGroup.GetFormattedText("ExtendedGuildStatisticsDescription", "You can separately share your data to create advanced statistics. Here, the data that can be determined via your API key is used to create statistics that compare the individual guild members.", context.Client.CurrentUser.Mention))
                      .AddField(LocalizationGroup.GetText("RefreshTitle", "State"), LocalizationGroup.GetFormattedText("RefreshDescription", "As of: {0}", new DateTime(2022, 4, 28).ToString("d", LocalizationGroup.CultureInfo)));

        await context.ReplyAsync(embed: builder.Build()).ConfigureAwait(false);
    }

    #endregion // Methods
}