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

    /// <summary>
    /// Returning the message
    /// </summary>
    /// <returns>Message</returns>
    public override string GetMessage() => LocalizationGroup.GetText("ChooseRoleTitle", "Role selection");

    /// <summary>
    /// Returning the placeholder
    /// </summary>
    /// <returns>Placeholder</returns>
    public override string GetPlaceholder() => LocalizationGroup.GetText("ChooseRoleDescription", "Please choose one of the following roles...");

    /// <summary>
    /// Returns the select menu entries which should be added to the message
    /// </summary>
    /// <returns>Reactions</returns>
    public override IReadOnlyList<SelectMenuEntryData<ulong>> GetEntries()
    {
        if (_entries == null)
        {
            _entries = new List<SelectMenuEntryData<ulong>>();

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

    /// <summary>
    /// Default case if none of the given buttons is used
    /// </summary>
    /// <returns>Result</returns>
    protected override ulong DefaultFunc() => throw new InvalidOperationException();

    #endregion // DialogEmbedMessageElementBase<long?>
}