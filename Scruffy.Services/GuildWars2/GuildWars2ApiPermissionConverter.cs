using System.Collections.Generic;
using System.Linq;

using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.Data.Json.GuildWars2.Core;
using Scruffy.Services.Account;
using Scruffy.Services.Core;

namespace Scruffy.Services.GuildWars2;

/// <summary>
/// Converting from or to <see cref="GuildWars2ApiPermission"/>
/// </summary>
public static class GuildWars2ApiPermissionConverter
{
    /// <summary>
    /// Get the permissions of the api key
    /// </summary>
    /// <param name="permissions">Permission strings</param>
    /// <returns>Permissions</returns>
    public static GuildWars2ApiPermission ToPermission(IEnumerable<string> permissions)
    {
        return permissions.Aggregate(GuildWars2ApiPermission.None,
                                     (combinedPermissions, permissionString) =>
                                     {
                                         var permission = permissionString switch
                                         {
                                             TokenInformation.Permission.Account => GuildWars2ApiPermission.Account,
                                             TokenInformation.Permission.Builds => GuildWars2ApiPermission.Builds,
                                             TokenInformation.Permission.Characters => GuildWars2ApiPermission.Characters,
                                             TokenInformation.Permission.Guilds => GuildWars2ApiPermission.Guilds,
                                             TokenInformation.Permission.Inventories => GuildWars2ApiPermission.Inventories,
                                             TokenInformation.Permission.Progression => GuildWars2ApiPermission.Progression,
                                             TokenInformation.Permission.PvP => GuildWars2ApiPermission.PvP,
                                             TokenInformation.Permission.TradingPost => GuildWars2ApiPermission.TradingPost,
                                             TokenInformation.Permission.Unlocks => GuildWars2ApiPermission.Unlocks,
                                             TokenInformation.Permission.Wallet => GuildWars2ApiPermission.Wallet,
                                             _ => GuildWars2ApiPermission.None
                                         };

                                         if (permission == GuildWars2ApiPermission.None)
                                         {
                                             LoggingService.AddServiceLogEntry(LogEntryLevel.Error, nameof(AccountAdministrationService), "Unknown permission value", permissionString, null);
                                         }

                                         combinedPermissions |= permission;

                                         return combinedPermissions;
                                     });
    }
}