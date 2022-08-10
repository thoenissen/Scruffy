using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// Selection of a role
/// </summary>
public class RaidRoleSelectionDialogElement : DialogMultiSelectSelectMenuElementBase<long>
{
    #region Fields

    /// <summary>
    /// Raid roles service
    /// </summary>
    private readonly RaidRolesService _raidRoleService;

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
    public RaidRoleSelectionDialogElement(LocalizationService localizationService, RaidRolesService raidRolesService)
        : base(localizationService)
    {
        _raidRoleService = raidRolesService;
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
    protected override int MaxValues => 2;

    /// <summary>
    /// Returning the message
    /// </summary>
    /// <returns>Message</returns>
    public override Task<string> GetMessage() => Task.FromResult(CommandContext.User.Mention + " " + LocalizationGroup.GetText("ChooseMainRoleTitle", "Role selection"));

    /// <summary>
    /// Returning the placeholder
    /// </summary>
    /// <returns>Placeholder</returns>
    public override string GetPlaceholder() => LocalizationGroup.GetText("ChooseMainRoleDescription", "Choose up to two of the following roles...");

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

    #endregion // DialogMultiSelectSelectMenuElementBase<long?>
}