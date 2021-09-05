using System.Net;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Scruffy.Services.Calendar;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord.Attributes;
using Scruffy.Services.CoreData;
using Scruffy.Services.Debug;
using Scruffy.Services.GuildAdministration;
using Scruffy.Services.GuildWars2;
using Scruffy.Services.Raid;

namespace Scruffy.Commands
{
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
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public DebugCommandModule(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

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
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public DebugDumpModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

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
        public class DebugRaidModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public DebugRaidModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

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
        [HelpOverviewCommand(HelpOverviewCommandAttribute.OverviewType.Developer)]
        [ModuleLifespan(ModuleLifespan.Transient)]
        public class DebugListModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public DebugListModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

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

            #endregion // Methods
        }

        #endregion // List

        #region Calendar

        /// <summary>
        /// Listing
        /// </summary>
        [Group("calendar")]
        [Aliases("c")]
        [ModuleLifespan(ModuleLifespan.Transient)]
        public class DebugCalendarModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public DebugCalendarModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

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
        public class DebugUserModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public DebugUserModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

            #region Properties

            /// <summary>
            /// User management service
            /// </summary>
            public UserManagementService UserManagementService { get; set; }

            #endregion // Properties

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
                await UserManagementService.CheckUserAsync(discordUser.Id)
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
        public class DebugGuildModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public DebugGuildModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

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

            #endregion // Methods
        }

        #endregion // Raid

        #region Import

        /// <summary>
        /// Import
        /// </summary>
        [Group("import")]
        [Aliases("i")]
        [ModuleLifespan(ModuleLifespan.Transient)]
        public class DebugImportModule : LocatedCommandModuleBase
        {
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public DebugImportModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

            #region Properties

            /// <summary>
            /// Worlds
            /// </summary>
            public WorldsService WorldsService { get; set; }

            #endregion // Properties

            #region Methods

            /// <summary>
            /// Import worlds
            /// </summary>
            /// <param name="commandContext">Current command context</param>
            /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
            [Command("worlds")]
            public async Task ExecuteSpecialRankJob(CommandContext commandContext)
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
            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="localizationService">Localization service</param>
            public DebugNetworkModule(LocalizationService localizationService)
                : base(localizationService)
            {
            }

            #endregion // Constructor

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
    }
}
