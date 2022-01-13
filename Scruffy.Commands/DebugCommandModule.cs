using System.Diagnostics;
using System.Globalization;
using System.Net;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Scruffy.Services.Calendar;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Discord.Attributes;
using Scruffy.Services.Debug;
using Scruffy.Services.Guild;
using Scruffy.Services.Guild.Jobs;
using Scruffy.Services.GuildWars2;
using Scruffy.Services.GuildWars2.Jobs;
using Scruffy.Services.Raid;

namespace Scruffy.Commands;

/// <summary>
/// Debug commands
/// </summary>
[Group("debug")]
[Aliases("d")]
[RequireDeveloperPermissions]
[ModuleLifespan(ModuleLifespan.Transient)]
[HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
public class DebugCommandModule : LocatedCommandModuleBase
{
    #region Methods

    /// <summary>
    /// List roles
    /// </summary>
    /// <param name="commandContext">Current command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("info")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public async Task Info(CommandContext commandContext)
    {
        var embed = new DiscordEmbedBuilder();

        embed.AddField("Information", $"Start time: {Process.GetCurrentProcess()?.StartTime.ToString("g", LocalizationGroup.CultureInfo)}");

        await commandContext.Channel
                            .SendMessageAsync(embed)
                            .ConfigureAwait(false);
    }

    #endregion // Methods

    #region Dump

    /// <summary>
    /// Listing
    /// </summary>
    [Group("dump")]
    [Aliases("d")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugDumpModule : LocatedCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Debug-Service
        /// </summary>
        public DebugService DebugService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// List roles
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("text")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
        public async Task Roles(CommandContext commandContext)
        {
            await DebugService.DumpText(commandContext)
                              .ConfigureAwait(false);
        }

        #endregion // Methods
    }

    #endregion // Dump

    #region Raid

    /// <summary>
    /// Listing
    /// </summary>
    [Group("raid")]
    [Aliases("r")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugRaidModule : LocatedCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Message builder
        /// </summary>
        public RaidMessageBuilder MessageBuilder { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// List roles
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="configurationId">Id of the configuration</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("refresh_message")]
        public async Task Roles(CommandContext commandContext, long configurationId)
        {
            await MessageBuilder.RefreshMessageAsync(configurationId)
                                .ConfigureAwait(false);

            await commandContext.Message
                                .DeleteAsync()
                                .ConfigureAwait(false);
        }

        #endregion // Methods
    }

    #endregion // Raid

    #region List

    /// <summary>
    /// Listing
    /// </summary>
    [Group("list")]
    [Aliases("l")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugListModule : LocatedCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Debug-Service
        /// </summary>
        public DebugService DebugService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// List roles
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("roles")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
        public async Task Roles(CommandContext commandContext)
        {
            await DebugService.ListRoles(commandContext)
                              .ConfigureAwait(false);
        }

        /// <summary>
        /// List users
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("users")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
        public async Task Users(CommandContext commandContext)
        {
            await DebugService.ListUsers(commandContext)
                              .ConfigureAwait(false);
        }

        /// <summary>
        /// List emojis
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("emojis")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
        public async Task Emojis(CommandContext commandContext)
        {
            await DebugService.ListEmojis(commandContext)
                              .ConfigureAwait(false);
        }

        /// <summary>
        /// List channels
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("channels")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
        public async Task Channels(CommandContext commandContext)
        {
            await DebugService.ListChannels(commandContext)
                              .ConfigureAwait(false);
        }

        /// <summary>
        /// List commands
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("commands")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
        public async Task Commands(CommandContext commandContext)
        {
            await DebugService.ListCommands(commandContext)
                              .ConfigureAwait(false);
        }

        #endregion // Methods
    }

    #endregion // List

    #region Account

    /// <summary>
    /// Guild
    /// </summary>
    [Group("account")]
    [Aliases("a")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugAccountModule : LocatedCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Debug-Service
        /// </summary>
        public DebugService DebugService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Refresh accounts
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("refresh")]
        public Task ExecuteSpecialRankJob(CommandContext commandContext) => DebugService.RefreshAccount();

        #endregion // Methods
    }

    #endregion // Account

    #region Guild Wars

    /// <summary>
    /// Guild
    /// </summary>
    [Group("gw")]
    [Aliases("g")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugGuildWarsModule : LocatedCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Debug-Service
        /// </summary>
        public AchievementService AchievementService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Import achievements
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("import_achievements")]
        public Task ImportAchievements(CommandContext commandContext) => AchievementService.ImportAchievements();

        /// <summary>
        /// Import achievements
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("import_account_achievements")]
        public Task ImportAccountAchievements(CommandContext commandContext) => new AchievementImportJob().ExecuteAsync();

        /// <summary>
        /// Import achievements
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="accountName">Account name</param>
        /// <param name="apiKey">API-Key</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("import_account_achievements")]
        public Task ImportAccountAchievements(CommandContext commandContext, string accountName, string apiKey) => AchievementService.ImportAccountAchievements(accountName, apiKey);

        #endregion // Methods
    }

    #endregion // Guild Wars

    #region Calendar

    /// <summary>
    /// Listing
    /// </summary>
    [Group("calendar")]
    [Aliases("c")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugCalendarModule : LocatedCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Message builder
        /// </summary>
        public CalendarMessageBuilderService MessageBuilder { get; set; }

        /// <summary>
        /// Message builder
        /// </summary>
        public CalendarScheduleService ScheduleService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Refresh calendar message
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("refresh_appointments")]
        public async Task RefreshAppointments(CommandContext commandContext)
        {
            await ScheduleService.CreateAppointments(commandContext.Guild.Id)
                                 .ConfigureAwait(false);

            await commandContext.Message
                                .DeleteAsync()
                                .ConfigureAwait(false);
        }

        /// <summary>
        /// Refresh calendar message
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("refresh_message")]
        public async Task RefreshMessage(CommandContext commandContext)
        {
            await MessageBuilder.RefreshMessages(commandContext.Guild.Id)
                                .ConfigureAwait(false);

            await commandContext.Message
                                .DeleteAsync()
                                .ConfigureAwait(false);
        }

        /// <summary>
        /// Refresh motd
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("refresh_motd")]
        public async Task RefreshMotd(CommandContext commandContext)
        {
            await MessageBuilder.RefreshMotds(commandContext.Guild.Id)
                                .ConfigureAwait(false);

            await commandContext.Message
                                .DeleteAsync()
                                .ConfigureAwait(false);
        }

        #endregion // Methods
    }

    #endregion // Raid

    #region User

    /// <summary>
    /// Listing
    /// </summary>
    [Group("user")]
    [Aliases("u")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugUserModule : LocatedCommandModuleBase
    {
        #region Methods

        /// <summary>
        /// Refresh calendar message
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="discordUser">User</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("add")]
        public async Task RefreshAppointments(CommandContext commandContext, DiscordUser discordUser)
        {
            await UserManagementService.CheckDiscordAccountAsync(discordUser.Id)
                                       .ConfigureAwait(false);

            await commandContext.Message
                                .CreateReactionAsync(DiscordEmojiService.GetCheckEmoji(commandContext.Client))
                                .ConfigureAwait(false);
        }

        #endregion // Methods
    }

    #endregion // Raid

    #region Guild

    /// <summary>
    /// Guild
    /// </summary>
    [Group("guild")]
    [Aliases("g")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugGuildModule : LocatedCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Guild rank service
        /// </summary>
        public GuildRankService GuildRankService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Refresh calendar message
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("specialrank_job")]
        public async Task ExecuteSpecialRankJob(CommandContext commandContext)
        {
            var job = new GuildSpecialRankPointsJob();

            await job.ExecuteAsync()
                     .ConfigureAwait(false);
        }

        /// <summary>
        /// Import members
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("import_members")]
        public async Task ImportMembers(CommandContext commandContext)
        {
            await GuildRankService.ImportGuildRanks(null)
                                  .ConfigureAwait(false);
        }

        /// <summary>
        /// Refresh discord roles
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("refresh_roles")]
        public async Task RefreshDiscordRoles(CommandContext commandContext)
        {
            await GuildRankService.RefreshDiscordRoles(null)
                                  .ConfigureAwait(false);
        }

        /// <summary>
        /// Refresh guild member rank
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="guidId">Id of the guild</param>
        /// <param name="accountName">Account name</param>
        /// <param name="rank">rank</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("refresh_rank")]
        public async Task RefreshRank(CommandContext commandContext, long guidId, string accountName, string rank)
        {
            await GuildRankService.RefreshDiscordRank(guidId, accountName, rank)
                                  .ConfigureAwait(false);
        }

        /// <summary>
        /// Refresh current rank points
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("refresh_rank_points")]
        public async Task RefreshCurrentPoints(CommandContext commandContext)
        {
            await GuildRankService.RefreshCurrentPoints(null)
                                  .ConfigureAwait(false);
        }

        #endregion // Methods
    }

    #endregion // Guild

    #region Import

    /// <summary>
    /// Import
    /// </summary>
    [Group("import")]
    [Aliases("i")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugImportModule : LocatedCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Worlds
        /// </summary>
        public WorldsService WorldsService { get; set; }

        /// <summary>
        /// Items
        /// </summary>
        public ItemsService ItemsService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Import worlds
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("worlds")]
        public async Task ImportWorlds(CommandContext commandContext)
        {
            if (await WorldsService.ImportWorlds()
                                   .ConfigureAwait(false))
            {
                await commandContext.Message
                                    .CreateReactionAsync(DiscordEmojiService.GetCheckEmoji(commandContext.Client))
                                    .ConfigureAwait(false);
            }
            else
            {
                await commandContext.Message
                                    .CreateReactionAsync(DiscordEmojiService.GetCrossEmoji(commandContext.Client))
                                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Import worlds
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("items")]
        public async Task ImportItems(CommandContext commandContext)
        {
            if (await ItemsService.ImportItems()
                                  .ConfigureAwait(false))
            {
                await commandContext.Message
                                    .CreateReactionAsync(DiscordEmojiService.GetCheckEmoji(commandContext.Client))
                                    .ConfigureAwait(false);
            }
            else
            {
                await commandContext.Message
                                    .CreateReactionAsync(DiscordEmojiService.GetCrossEmoji(commandContext.Client))
                                    .ConfigureAwait(false);
            }
        }

        #endregion // Methods
    }

    #endregion // Raid

    #region Network

    /// <summary>
    /// Import
    /// </summary>
    [Group("network")]
    [Aliases("n")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugNetworkModule : LocatedCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Worlds
        /// </summary>
        public WorldsService WorldsService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// DNS-Information overview
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="domain">Domain name</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("dnsinfo")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
        public async Task GetDnsInformation(CommandContext commandContext, string domain)
        {
            var embed = new DiscordEmbedBuilder().WithTitle("DNS Information")
                                                 .WithDescription(domain);

            var addresses = await Dns.GetHostAddressesAsync(domain)
                                     .ConfigureAwait(false);

            var stringBuilder = new StringBuilder();

            foreach (var address in addresses)
            {
                stringBuilder.AppendLine(address.ToString());
            }

            stringBuilder.Append("\u200B");

            embed.AddField("IP-Addresses", stringBuilder.ToString());

            await commandContext.RespondAsync(embed)
                                .ConfigureAwait(false);
        }

        #endregion // Methods
    }

    #endregion // Raid

    #region Log

    /// <summary>
    /// Log
    /// </summary>
    [Group("log")]
    [Aliases("d")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugLogModule : LocatedCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Debug-Service
        /// </summary>
        public DebugService DebugService { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Log entry information
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="id">Id</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("entry")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
        public async Task ShowLogEntry(CommandContext commandContext, int id)
        {
            await DebugService.PostLogEntry(commandContext, id)
                              .ConfigureAwait(false);
        }

        /// <summary>
        /// Log entry information
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("overview")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
        public async Task ShowLogOverview(CommandContext commandContext)
        {
            await DebugService.PostLogOverview(commandContext.Channel, DateTime.Today, false)
                              .ConfigureAwait(false);
        }

        /// <summary>
        /// Log entry information
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="date">Date</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("overview")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
        public async Task ShowLogOverview(CommandContext commandContext, string date)
        {
            if (DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateParsed) == false)
            {
                dateParsed = DateTime.Today;
            }

            await DebugService.PostLogOverview(commandContext.Channel, dateParsed, false)
                              .ConfigureAwait(false);
        }

        #endregion // Methods
    }

    #endregion // Log
}