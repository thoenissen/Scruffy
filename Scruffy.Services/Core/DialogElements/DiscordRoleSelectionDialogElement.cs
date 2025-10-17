using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Core.DialogElements;

/// <summary>
/// Selection of a role
/// </summary>
public class DiscordRoleSelectionDialogElement : DialogSelectMenuElementBase<ulong>
{
    #region Fields

    /// <summary>
    /// Roles
    /// </summary>
    private List<SelectMenuEntryData<ulong>> _entries;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public DiscordRoleSelectionDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Additional role filter
    /// </summary>
    public virtual Func<IRole, bool> RoleFilter { get; set; }

    #endregion // Properties

    #region DialogEmbedMessageElementBase<long?>

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetText("ChooseRoleTitle", "Role selection");

    /// <inheritdoc/>
    public override string GetPlaceholder() => LocalizationGroup.GetText("ChooseRoleDescription", "Please choose one of the following roles...");

    /// <inheritdoc/>
    public override IReadOnlyList<SelectMenuEntryData<ulong>> GetEntries()
    {
        if (_entries == null)
        {
            _entries = [];

            foreach (var role in CommandContext.Guild
                                               .Roles
                                               .Where(obj => obj.IsManaged == false
                                                          && obj.Id != obj.Guild.Id)
                                               .OrderByDescending(obj => obj.Position))
            {
                if (RoleFilter?.Invoke(role) != false)
                {
                    _entries.Add(new SelectMenuEntryData<ulong>
                                 {
                                     CommandText = role.Name,
                                     Emote = role.Emoji.Name != null
                                                 ? role.Emoji
                                                 : null,
                                     Response = () => Task.FromResult(role.Id)
                                 });
                }
            }
        }

        return _entries;
    }

    /// <inheritdoc/>
    protected override ulong DefaultFunc() => throw new InvalidOperationException();

    #endregion // DialogEmbedMessageElementBase<long?>
}