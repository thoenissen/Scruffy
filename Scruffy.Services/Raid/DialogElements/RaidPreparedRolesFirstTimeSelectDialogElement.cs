using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// First prepared raid role selection
/// </summary>
public class RaidPreparedRolesFirstTimeSelectDialogElement : DialogMultiSelectSelectMenuElementBase<long>
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
    public RaidPreparedRolesFirstTimeSelectDialogElement(LocalizationService localizationService, RaidRolesService raidRolesService)
        : base(localizationService)
    {
        _raidRoleService = raidRolesService;
    }

    #endregion // Constructor

    #region DialogEmbedMultiSelectSelectMenuElementBase<long>

    /// <inheritdoc/>
    protected override int MinValues => 1;

    /// <inheritdoc/>
    protected override int MaxValues => 9;

    /// <inheritdoc/>
    public override Task<string> GetMessage() => Task.FromResult(LocalizationGroup.GetFormattedText("Message", "{0}, to participate in a raid you must first select the roles you have prepared for it. You can customize your roles at any time using the `/raid roles` command. Please choose your roles in the following selection: ", CommandContext.User.Mention));

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

#endregion // DialogEmbedMultiSelectSelectMenuElementBase<long>