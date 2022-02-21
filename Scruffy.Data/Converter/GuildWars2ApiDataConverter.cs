using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.Data.Json.GuildWars2.Core;

namespace Scruffy.Data.Converter;

/// <summary>
/// Converting data from the Guild Wars 2 API
/// </summary>
public static class GuildWars2ApiDataConverter
{
    /// <summary>
    /// Get the permissions of the api key
    /// </summary>
    /// <param name="permissions">Permission strings</param>
    /// <returns>Permissions</returns>
    public static GuildWars2ApiPermission ToPermission(IEnumerable<string> permissions)
    {
        return permissions?.Aggregate(GuildWars2ApiPermission.None,
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

                                         combinedPermissions |= permission;

                                         return combinedPermissions;
                                     })
            ?? GuildWars2ApiPermission.None;
    }

    /// <summary>
    /// Get the item type
    /// </summary>
    /// <param name="typeString">Type string</param>
    /// <returns>Type</returns>
    public static GuildWars2ItemType ToItemType(string typeString)
    {
        var type = typeString switch
                   {
                       "Armor" => GuildWars2ItemType.Armor,
                       "Back" => GuildWars2ItemType.Back,
                       "Bag" => GuildWars2ItemType.Bag,
                       "Consumable" => GuildWars2ItemType.Consumable,
                       "Container" => GuildWars2ItemType.Container,
                       "CraftingMaterial" => GuildWars2ItemType.CraftingMaterial,
                       "Gathering" => GuildWars2ItemType.Gathering,
                       "Gizmo" => GuildWars2ItemType.Gizmo,
                       "Key" => GuildWars2ItemType.Key,
                       "MiniPet" => GuildWars2ItemType.MiniPet,
                       "Tool" => GuildWars2ItemType.Tool,
                       "Trait" => GuildWars2ItemType.Trait,
                       "Trinket" => GuildWars2ItemType.Trinket,
                       "Trophy" => GuildWars2ItemType.Trophy,
                       "UpgradeComponent" => GuildWars2ItemType.UpgradeComponent,
                       "Weapon" => GuildWars2ItemType.Weapon,
                       _ => GuildWars2ItemType.Unknown
                   };

        return type;
    }
}