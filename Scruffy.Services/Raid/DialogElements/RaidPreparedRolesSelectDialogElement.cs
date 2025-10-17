using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// Prepared special raid role selection
/// </summary>
public class RaidPreparedRolesSelectDialogElement : DialogEmbedMultiSelectSelectMenuElementBase<long>
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
    public RaidPreparedRolesSelectDialogElement(LocalizationService localizationService, RaidRolesService raidRolesService, UserManagementService userManagementService)
        : base(localizationService)
    {
        _raidRoleService = raidRolesService;
        _userManagementService = userManagementService;

        IsEphermeral = true;
    }

    #endregion // Constructor

    #region DialogEmbedMultiSelectSelectMenuElementBase<long>

    /// <inheritdoc/>
    protected override int MinValues => 1;

    /// <inheritdoc/>
    protected override int MaxValues => 9;

    /// <inheritdoc/>
    public override async Task<EmbedBuilder> GetMessage()
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var user = await _userManagementService.GetUserByDiscordAccountId(CommandContext.User)
                                                   .ConfigureAwait(false);

            var raidUserRoles = dbFactory.GetRepository<RaidUserRoleRepository>()
                                         .GetQuery()
                                         .Where(obj => obj.UserId == user.Id)
                                         .OrderBy(obj => obj.RoleId)
                                         .Select(obj => obj.RaidRole)
                                         .ToList();

            var userRolesMsg = new StringBuilder();

            foreach (var role in raidUserRoles)
            {
                userRolesMsg.AppendLine(DiscordEmoteService.GetGuildEmote(CommandContext.Client, role.DiscordEmojiId) + " " + _raidRoleService.GetDescriptionAsText(role));
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

    /// <inheritdoc/>
    public override string GetPlaceholder() => LocalizationGroup.GetText("RoleSelectionPlaceHolder", "Choose your roles...");

    /// <inheritdoc/>
    public override IReadOnlyList<SelectMenuOptionData> GetEntries()
    {
        if (_entries == null)
        {
            _entries = [];

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var userId = _userManagementService.GetUserByDiscordAccountId(CommandContext.User).Result.Id;

                var userRoles = dbFactory.GetRepository<RaidUserRoleRepository>()
                                         .GetQuery()
                                         .Where(obj => obj.UserId == userId)
                                         .Select(obj => obj.RoleId)
                                         .ToList();

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
                                     IsDefault = userRoles.Contains(role.Id),
                                 });
                }
            }
        }

        return _entries;
    }
}

#endregion // DialogEmbedMultiSelectSelectMenuElementBase<long>