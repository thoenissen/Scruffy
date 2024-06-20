using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Enumerations.Guild;
using Scruffy.Services.Core.DialogElements;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Selection of a discord role which should be added
/// </summary>
public class GuildActivityDiscordMessageAddDialogElement : DiscordRoleSelectionDialogElement
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildActivityDiscordMessageAddDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DiscordRoleSelectionDialogElement

    /// <inheritdoc/>
    public override Func<IRole, bool> RoleFilter
    {
        get
        {
            if (base.RoleFilter == null)
            {
                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    var existingRoles = dbFactory.GetRepository<GuildDiscordActivityPointsAssignmentRepository>()
                                                 .GetQuery()
                                                 .Where(obj => obj.Guild.DiscordServerId == CommandContext.Guild.Id
                                                            && obj.Type == DiscordActivityPointsType.Message)
                                                 .Select(obj => obj.RoleId)
                                                 .ToList();

                    base.RoleFilter = role => existingRoles.Contains(role.Id) == false;
                }
            }

            return base.RoleFilter;
        }
        set => base.RoleFilter = value;
    }

    #endregion // DiscordRoleSelectionDialogElement
}