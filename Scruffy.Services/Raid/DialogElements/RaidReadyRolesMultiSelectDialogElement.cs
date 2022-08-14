using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid.DialogElements
{
    /// <summary>
    /// RaidReadyRolesMultiSelectDialogElement
    /// </summary>
    public class RaidReadyRolesMultiSelectDialogElement : DialogEmbedMultiSelectSelectMenuElementBase<long>
    {
        #region Fields

        /// <summary>
        /// Raid roles service
        /// </summary>
        private readonly RaidRolesService _raidRoleService;

        /// <summary>
        /// UserManagementService
        /// </summary>
        private readonly UserManagementService _userManagememtService;

        /// <summary>
        /// Roles
        /// </summary>
        private List<SelectMenuOptionData> _entries;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="raidRolesService">Raid roles service</param>
        /// <param name="userManagemantService">UserManagementService</param>
        public RaidReadyRolesMultiSelectDialogElement(LocalizationService localizationService, RaidRolesService raidRolesService, UserManagementService userManagemantService)
            : base(localizationService)
        {
            _raidRoleService = raidRolesService;
            _userManagememtService = userManagemantService;
        }

        #endregion // Constructor

        #region DialogMultiSelectSelectMenuElementBase<long>

        /// <summary>
        /// Min values
        /// </summary>
        protected override int MinValues => 1;

        /// <summary>
        /// Max values
        /// </summary>
        protected override int MaxValues => 9;

        /// <summary>
        /// Embed
        /// </summary>
        /// <returns>Task Embed</returns>
        public override async Task<EmbedBuilder> GetMessage()
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var user = await _userManagememtService.GetUserByDiscordAccountId(CommandContext.User.Id)
                                          .ConfigureAwait(false);

                var raidUserRoles = dbFactory.GetRepository<RaidUserRoleRepository>()
                                           .GetQuery()
                                           .Where(obj => obj.UserId == user.Id)
                                           .Select(obj => obj.RaidRole)
                                           .ToList();

                var userRolesMsg = new StringBuilder();

                foreach (var role in raidUserRoles)
                {
                    userRolesMsg.AppendLine(_raidRoleService.GetDescriptionAsText(role));
                }
                var embedBuilder = new EmbedBuilder()
                    .WithTitle(LocalizationGroup.GetText("RaidReadyRolesTitle", "Raid-Ready Roles"))
                    .AddField(LocalizationGroup.GetText("RaidReadyRolesField1", "Your Raid-Ready Roles are:"), value: userRolesMsg.ToString())
                    .WithColor(Color.Green)
                    .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                    .WithTimestamp(DateTime.Now);

                return embedBuilder;
            }
        }

        /// <summary>
        /// Returning the placeholder
        /// </summary>
        /// <returns>Placeholder</returns>
        public override string GetPlaceholder() => LocalizationGroup.GetText("ChooseReadyRoleDescription", "Choose your roles...");

        /// <summary>
        /// Returns the select menu entries which should be added to the message
        /// </summary>
        /// <returns>Reactions</returns>
        public override IReadOnlyList<SelectMenuOptionData> GetEntries()
        {
            if (_entries == null)
            {
                _entries = new List<SelectMenuOptionData>();

                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    var roles = dbFactory.GetRepository<RaidRoleRepository>()
                                         .GetQuery()
                                         .OrderBy(obj => obj.Id)
                                         .ToList();

                    foreach (var role in roles)
                    {
                        _entries.Add(new SelectMenuOptionData
                        {
                            Label = _raidRoleService.GetDescriptionAsText(role),
                            Emote = DiscordEmoteService.GetGuildEmote(CommandContext.Client, role.DiscordEmojiId),
                            Value = role.Id.ToString(),
                        });
                    }
                }
            }

            return _entries;
        }
    }

    #endregion
}