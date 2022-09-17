using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// Selection of prepare special raid roles
/// </summary>
public class RaidPreparedSpecialRolesSelectDialogElement : DialogEmbedMultiSelectSelectMenuElementBase<long>
{
    #region Fields

    /// <summary>
    /// Raid roles service
    /// </summary>
    private readonly RaidRolesService _raidRoleService;

    /// <summary>
    /// UserManagementService
    /// </summary>
    private readonly UserManagementService _userManagementService;

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
    /// <param name="userManagementService">UserManagementService</param>
    public RaidPreparedSpecialRolesSelectDialogElement(LocalizationService localizationService, RaidRolesService raidRolesService, UserManagementService userManagementService)
        : base(localizationService)
    {
        _raidRoleService = raidRolesService;
        _userManagementService = userManagementService;
    }

    #endregion // Constructor

    #region DialogEmbedMultiSelectSelectMenuElementBase<long>

    /// <summary>
    /// Min values
    /// </summary>
    protected override int MinValues => 1;

    /// <summary>
    /// Max values
    /// </summary>
    protected override int MaxValues => 4;

    /// <summary>
    /// Embed
    /// </summary>
    /// <returns>Task Embed</returns>
    public override async Task<EmbedBuilder> GetMessage()
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var user = await _userManagementService.GetUserByDiscordAccountId(CommandContext.User)
                                                   .ConfigureAwait(false);

            var raidUserRoles = dbFactory.GetRepository<RaidUserSpecialRoleRepository>()
                                         .GetQuery()
                                         .Where(obj => obj.UserId == user.Id)
                                         .OrderBy(obj => obj.SpecialRoleId)
                                         .Select(obj => obj.RaidSpecialRole)
                                         .ToList();

            var userRolesMsg = new StringBuilder();

            foreach (var role in raidUserRoles)
            {
                userRolesMsg.AppendLine(DiscordEmoteService.GetGuildEmote(CommandContext.Client, role.DiscordEmojiId) + " " + role.Description);
            }

            if (userRolesMsg.Length == 0)
            {
                userRolesMsg.Append("\u200b");
            }

            var embedBuilder = new EmbedBuilder().WithTitle(LocalizationGroup.GetText("EmbedTitle", "Raid role selection"))
                                                 .WithDescription(LocalizationGroup.GetText("EmbedDescription", "With the this assistant you are able to select all roles which you have prepared for raiding. The following roles are already selected."))
                                                 .AddField(LocalizationGroup.GetText("RolesTitle", "Roles"), userRolesMsg.ToString())
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
    public override string GetPlaceholder() => LocalizationGroup.GetText("RoleSelectionPlaceHolder", "Choose your roles...");

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
                var roles = dbFactory.GetRepository<RaidSpecialRoleRepository>()
                                     .GetQuery()
                                     .OrderBy(obj => obj.Id)
                                     .ToList();

                foreach (var role in roles)
                {
                    _entries.Add(new SelectMenuOptionData
                                 {
                                     Label = role.Description,
                                     Emote = DiscordEmoteService.GetGuildEmote(CommandContext.Client, role.DiscordEmojiId),
                                     Value = role.Id.ToString(),
                                 });
                }
            }
        }

        return _entries;
    }
}

#endregion // DialogEmbedMultiSelectSelectMenuElementBase<long>