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
    /// Roles
    /// </summary>
    private List<SelectMenuEntryData<long?>> _entries;

    /// <summary>
    /// Id of the main role
    /// </summary>
    private long? _mainRoleId;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="mainRoleId">Id of the main role</param>
    public RaidRoleSelectionDialogElement(LocalizationService localizationService, long? mainRoleId)
        : base(localizationService)
    {
        _mainRoleId = mainRoleId;
    }

    #endregion // Constructor

    #region DialogEmbedMessageElementBase<long?>

    /// <summary>
    /// Returning the message
    /// </summary>
    /// <returns>Message</returns>
    public override string GetMessage() => _mainRoleId == null
                                               ? LocalizationGroup.GetText("ChooseMainRoleTitle", "Role selection")
                                               : LocalizationGroup.GetText("ChooseSubRoleTitle", "Class selection");

    /// <summary>
    /// Returning the placeholder
    /// </summary>
    /// <returns>Placeholder</returns>
    public override string GetPlaceholder() => _mainRoleId == null
                                                   ? LocalizationGroup.GetText("ChooseMainRoleDescription", "Please choose one of the following roles...")
                                                   : LocalizationGroup.GetText("ChooseSubRoleDescription", "Please choose one of the following classes...");

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
                                     .Where(obj => obj.MainRoleId == _mainRoleId
                                                && obj.IsDeleted == false)
                                     .Select(obj => new
                                                    {
                                                        obj.Id,
                                                        obj.Description,
                                                        obj.DiscordEmojiId
                                                    })
                                     .OrderBy(obj => obj.Description)
                                     .ToList();

                _entries.Add(new SelectMenuEntryData<long?>
                             {
                                 CommandText = LocalizationGroup.GetText("NoRole", "No role specification"),
                                 Emote = null,
                                 Func = () => Task.FromResult<long?>(null)
                             });

                foreach (var role in roles)
                {
                    _entries.Add(new SelectMenuEntryData<long?>
                                 {
                                     CommandText = role.Description,
                                     Emote = DiscordEmoteService.GetGuildEmote(CommandContext.Client, role.DiscordEmojiId),
                                     Func = () => Task.FromResult<long?>(role.Id)
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