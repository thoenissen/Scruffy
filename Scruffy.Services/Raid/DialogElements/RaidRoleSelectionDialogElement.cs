using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// Selection of a role
/// </summary>
public class RaidRoleSelectionDialogElement : DialogSelectMenuElementBase<long?>
{
    #region Fields

    /// <summary>
    /// Raid roles service
    /// </summary>
    private readonly RaidRolesService _raidRoleService;

    /// <summary>
    /// Roles
    /// </summary>
    private List<SelectMenuEntryData<long?>> _entries;

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

    #region DialogEmbedMessageElementBase<long?>

    /// <summary>
    /// Returning the message
    /// </summary>
    /// <returns>Message</returns>
    public override string GetMessage() => CommandContext.User.Mention + " " + LocalizationGroup.GetText("ChooseMainRoleTitle", "Role selection");

    /// <summary>
    /// Returning the placeholder
    /// </summary>
    /// <returns>Placeholder</returns>
    public override string GetPlaceholder() => LocalizationGroup.GetText("ChooseMainRoleDescription", "Please choose one of the following roles...");

    /// <summary>
    /// Returns the select menu entries which should be added to the message
    /// </summary>
    /// <returns>Reactions</returns>
    public override IReadOnlyList<SelectMenuEntryData<long?>> GetEntries()
    {
        if (_entries == null)
        {
            _entries = new List<SelectMenuEntryData<long?>>();

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var roles = dbFactory.GetRepository<RaidRoleRepository>()
                                     .GetQuery()
                                     .OrderBy(obj => obj.Id)
                                     .ToList();

                _entries.Add(new SelectMenuEntryData<long?>
                             {
                                 CommandText = LocalizationGroup.GetText("NoRole", "No role specification"),
                                 Emote = null,
                                 Response = () => Task.FromResult<long?>(null)
                             });

                foreach (var role in roles)
                {
                    _entries.Add(new SelectMenuEntryData<long?>
                                 {
                                     CommandText = _raidRoleService.GetDescriptionAsText(role),
                                     Emote = DiscordEmoteService.GetGuildEmote(CommandContext.Client, role.DiscordEmojiId),
                                     Response = () => Task.FromResult<long?>(role.Id)
                                 });
                }
            }
        }

        return _entries;
    }

    /// <summary>
    /// Default case if none of the given buttons is used
    /// </summary>
    /// <returns>Result</returns>
    protected override long? DefaultFunc() => throw new InvalidOperationException();

    #endregion // DialogEmbedMessageElementBase<long?>
}