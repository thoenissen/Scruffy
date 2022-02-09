using System.Diagnostics;
using System.Globalization;
using System.Net;

using Discord;
using Discord.Commands;

using Scruffy.Commands.MessageComponents;
using Scruffy.Services.Calendar;
using Scruffy.Services.Debug;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;
using Scruffy.Services.Guild;
using Scruffy.Services.Guild.Jobs;
using Scruffy.Services.GuildWars2;
using Scruffy.Services.GuildWars2.Jobs;
using Scruffy.Services.Raid;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// Debug commands
/// </summary>
[Group("debug")]
[Alias("d")]
[RequireDeveloperPermissions]
[HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
public class DebugCommandModule : TextCommandModuleBase
{
    #region Methods

    /// <summary>
    /// List roles
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("info")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public async Task Info()
    {
        var embed = new EmbedBuilder();

        embed.AddField("Information", $"Start time: {Process.GetCurrentProcess().StartTime.ToString("g", LocalizationGroup.CultureInfo)}");

        await Context.Channel
                     .SendMessageAsync(embed: embed.Build())
                     .ConfigureAwait(false);
    }

    /// <summary>
    /// Wait for message
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("message")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public async Task Message()
    {
        await Context.Message
                     .ReplyAsync("Please answer!")
                     .ConfigureAwait(false);

        var message = await Context.Interaction
                                   .WaitForMessageAsync(obj => obj.Author.Id == Context.Message.Author.Id).ConfigureAwait(false);

        await message.ReplyAsync(message.Content)
                     .ConfigureAwait(false);
    }

    /// <summary>
    /// Wait for reaction
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("reaction")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public async Task Reaction()
    {
        var message = await Context.Message
                                   .ReplyAsync("Please react!")
                                   .ConfigureAwait(false);

        var reaction = await Context.Interaction
                                    .WaitForReactionAsync(message, Context.Message.Author).ConfigureAwait(false);

        await message.ReplyAsync(reaction.Emote.ToString())
                     .ConfigureAwait(false);
    }

    /// <summary>
    /// Command buttons
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("buttons")]
    public async Task Buttons()
    {
        var components = Context.Interaction.CreateTemporaryComponentContainer<int>(obj => true);
        await using (components.ConfigureAwait(false))
        {
            var builder = new ComponentBuilder();

            builder.WithButton("Test 1", components.AddComponent(1), ButtonStyle.Secondary, DiscordEmoteService.GetCheckEmote(Context.Client));
            builder.WithButton("Test 2", components.AddComponent(2), ButtonStyle.Secondary, DiscordEmoteService.GetCrossEmote(Context.Client));

            var message = await Context.Message
                                       .ReplyAsync("Buttons:", components: builder.Build())
                                       .ConfigureAwait(false);

            try
            {
                var button = await components.Task
                                             .ConfigureAwait(false);

                await button.Component
                            .RespondAsync("Button pressed: " + button)
                            .ConfigureAwait(false);
            }
            finally
            {
                await message.ModifyAsync(obj =>
                                          {
                                              obj.Content = "Interaction closed";
                                              obj.Components = new ComponentBuilder().Build();
                                          })
                             .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Permanent command buttons
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("pbuttons")]
    public async Task PermanentButton()
    {
        var builder = new ComponentBuilder();

        builder.WithButton("Ping 1",
                           Context.Interaction
                                  .GetPermanentCustomerId(DebugMessageComponentCommandModule.Group,
                                                          DebugMessageComponentCommandModule.CommandPing),
                           ButtonStyle.Primary,
                           DiscordEmoteService.GetBulletEmote(Context.Client));
        builder.WithButton("Ping 2",
                           Context.Interaction
                                  .GetPermanentCustomerId(DebugMessageComponentCommandModule.Group,
                                                          DebugMessageComponentCommandModule.CommandPing),
                           ButtonStyle.Secondary,
                           DiscordEmoteService.GetBulletEmote(Context.Client));

        await Context.Message
                     .ReplyAsync("Permanent Buttons:", components: builder.Build())
                     .ConfigureAwait(false);
    }

    /// <summary>
    /// Command select menu
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("selectmenu")]
    public async Task SelectMenu()
    {
        var components = Context.Interaction.CreateTemporaryComponentContainer<int>(obj => true);
        await using (components.ConfigureAwait(false))
        {
            var builder = new ComponentBuilder();

            builder.WithSelectMenu(new SelectMenuBuilder()
                                       .WithCustomId(components.AddComponent(1))
                                       .WithPlaceholder("Please select a option...")
                                       .AddOption("Option 0", "0", null, DiscordEmoteService.GetBulletEmote(Context.Client))
                                       .AddOption("Option 1", "1", null, DiscordEmoteService.GetEdit2Emote(Context.Client)));

            var message = await Context.Message
                                       .ReplyAsync("SelectMenu:", components: builder.Build())
                                       .ConfigureAwait(false);

            try
            {
                var selectMenu = await components.Task
                                                 .ConfigureAwait(false);

                await selectMenu.Component
                            .RespondAsync("SelectMenu selected: " + selectMenu.Component.Data)
                            .ConfigureAwait(false);
            }
            finally
            {
                await message.ModifyAsync(obj =>
                                          {
                                              obj.Content = "Interaction closed";
                                              obj.Components = new ComponentBuilder().Build();
                                          })
                             .ConfigureAwait(false);
            }
        }
    }

    #endregion // Methods

    #region Dump

    /// <summary>
    /// Listing
    /// </summary>
    [Group("dump")]
    [Alias("d")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugDumpModule : TextCommandModuleBase
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
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("text")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
        public async Task Roles()
        {
            await DebugService.DumpText(Context)
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
    [Alias("r")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugRaidModule : TextCommandModuleBase
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
        /// <param name="configurationId">Id of the configuration</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("refresh_message")]
        public async Task Roles(long configurationId)
        {
            await MessageBuilder.RefreshMessageAsync(configurationId)
                                .ConfigureAwait(false);

            await Context.Message
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
    [Alias("l")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugListModule : TextCommandModuleBase
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
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("roles")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
        public async Task Roles()
        {
            await DebugService.ListRoles(Context)
                              .ConfigureAwait(false);
        }

        /// <summary>
        /// List users
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("users")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
        public async Task Users()
        {
            await DebugService.ListUsers(Context)
                              .ConfigureAwait(false);
        }

        /// <summary>
        /// List emojis
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("emojis")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
        public async Task Emojis()
        {
            await DebugService.ListEmojis(Context)
                              .ConfigureAwait(false);
        }

        /// <summary>
        /// List channels
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("channels")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
        public async Task Channels()
        {
            await DebugService.ListChannels(Context)
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
    [Alias("a")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugAccountModule : TextCommandModuleBase
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
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("refresh")]
        public Task ExecuteSpecialRankJob() => DebugService.RefreshAccount();

        #endregion // Methods
    }

    #endregion // Account

    #region Guild Wars

    /// <summary>
    /// Guild
    /// </summary>
    [Group("gw")]
    [Alias("g")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugGuildWarsModule : TextCommandModuleBase
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
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("import_achievements")]
        public Task ImportAchievements() => AchievementService.ImportAchievements();

        /// <summary>
        /// Import achievements
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("import_account_achievements")]
        public Task ImportAccountAchievements() => new AchievementImportJob().ExecuteAsync();

        /// <summary>
        /// Import achievements
        /// </summary>
        /// <param name="accountName">Account name</param>
        /// <param name="apiKey">API-Key</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("import_account_achievements")]
        public Task ImportAccountAchievements(string accountName, string apiKey) => AchievementService.ImportAccountAchievements(accountName, apiKey);

        #endregion // Methods
    }

    #endregion // Guild Wars

    #region Calendar

    /// <summary>
    /// Listing
    /// </summary>
    [Group("calendar")]
    [Alias("c")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugCalendarModule : TextCommandModuleBase
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
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("refresh_appointments")]
        public async Task RefreshAppointments()
        {
            await ScheduleService.CreateAppointments(Context.Guild.Id)
                                 .ConfigureAwait(false);

            await Context.Message
                         .DeleteAsync()
                         .ConfigureAwait(false);
        }

        /// <summary>
        /// Refresh calendar message
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("refresh_message")]
        public async Task RefreshMessage()
        {
            await MessageBuilder.RefreshMessages(Context.Guild.Id)
                                .ConfigureAwait(false);

            await Context.Message
                         .DeleteAsync()
                         .ConfigureAwait(false);
        }

        /// <summary>
        /// Refresh motd
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("refresh_motd")]
        public async Task RefreshMotd()
        {
            await MessageBuilder.RefreshMotds(Context.Guild.Id)
                                .ConfigureAwait(false);

            await Context.Message
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
    [Alias("u")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugUserModule : TextCommandModuleBase
    {
        #region Methods

        /// <summary>
        /// Refresh calendar message
        /// </summary>
        /// <param name="discordUser">User</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("add")]
        public async Task RefreshAppointments(IUser discordUser)
        {
            await UserManagementService.CheckDiscordAccountAsync(discordUser.Id)
                                       .ConfigureAwait(false);

            await Context.Message
                         .AddReactionAsync(DiscordEmoteService.GetCheckEmote(Context.Client))
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
    [Alias("g")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugGuildModule : TextCommandModuleBase
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
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("specialrank_job")]
        public async Task ExecuteSpecialRankJob()
        {
            var job = new GuildSpecialRankPointsJob();

            await job.ExecuteAsync()
                     .ConfigureAwait(false);
        }

        /// <summary>
        /// Import members
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("import_members")]
        public async Task ImportMembers()
        {
            await GuildRankService.ImportGuildRanks(null)
                                  .ConfigureAwait(false);
        }

        /// <summary>
        /// Refresh discord roles
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("refresh_roles")]
        public async Task RefreshDiscordRoles()
        {
            await GuildRankService.RefreshDiscordRoles(null)
                                  .ConfigureAwait(false);
        }

        /// <summary>
        /// Refresh guild member rank
        /// </summary>
        /// <param name="guidId">Id of the guild</param>
        /// <param name="accountName">Account name</param>
        /// <param name="rank">rank</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("refresh_rank")]
        public async Task RefreshRank(long guidId, string accountName, string rank)
        {
            await GuildRankService.RefreshDiscordRank(guidId, accountName, rank)
                                  .ConfigureAwait(false);
        }

        /// <summary>
        /// Refresh current rank points
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("refresh_rank_points")]
        public async Task RefreshCurrentPoints()
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
    [Alias("i")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugImportModule : TextCommandModuleBase
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
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("worlds")]
        public async Task ImportWorlds()
        {
            if (await WorldsService.ImportWorlds()
                                   .ConfigureAwait(false))
            {
                await Context.Message
                             .AddReactionAsync(DiscordEmoteService.GetCheckEmote(Context.Client))
                             .ConfigureAwait(false);
            }
            else
            {
                await Context.Message
                             .AddReactionAsync(DiscordEmoteService.GetCrossEmote(Context.Client))
                             .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Import worlds
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("items")]
        public async Task ImportItems()
        {
            if (await ItemsService.ImportItems()
                                  .ConfigureAwait(false))
            {
                await Context.Message
                             .AddReactionAsync(DiscordEmoteService.GetCheckEmote(Context.Client))
                             .ConfigureAwait(false);
            }
            else
            {
                await Context.Message
                             .AddReactionAsync(DiscordEmoteService.GetCrossEmote(Context.Client))
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
    [Alias("n")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugNetworkModule : TextCommandModuleBase
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
        /// <param name="domain">Domain name</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("dnsinfo")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
        public async Task GetDnsInformation(string domain)
        {
            var embed = new EmbedBuilder().WithTitle("DNS Information")
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

            await Context.Message
                         .ReplyAsync(embed: embed.Build())
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
    [Alias("d")]
    [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
    public class DebugLogModule : TextCommandModuleBase
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
        /// <param name="id">Id</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("entry")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
        public async Task ShowLogEntry(int id)
        {
            await DebugService.PostLogEntry(Context, id)
                              .ConfigureAwait(false);
        }

        /// <summary>
        /// Log entry information
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("overview")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
        public async Task ShowLogOverview()
        {
            await DebugService.PostLogOverview(Context.Channel, DateTime.Today, false)
                              .ConfigureAwait(false);
        }

        /// <summary>
        /// Log entry information
        /// </summary>
        /// <param name="date">Date</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("overview")]
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
        public async Task ShowLogOverview(string date)
        {
            if (DateTime.TryParseExact(date,
                                       "yyyy-MM-dd",
                                       CultureInfo.InvariantCulture,
                                       DateTimeStyles.None,
                                       out var dateParsed) == false)
            {
                dateParsed = DateTime.Today;
            }

            await DebugService.PostLogOverview(Context.Channel, dateParsed, false)
                              .ConfigureAwait(false);
        }

        #endregion // Methods
    }

    #endregion // Log
}