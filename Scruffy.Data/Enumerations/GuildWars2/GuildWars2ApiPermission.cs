using System;

namespace Scruffy.Data.Enumerations.GuildWars2;

/// <summary>
/// Guild Wars 2 API Permission
/// </summary>
[Flags]
public enum GuildWars2ApiPermission : ulong
{
    /// <summary>
    /// Minimum required rights
    /// </summary>
    RequiredPermissions = Account | Characters | Progression,

    /// <summary>
    /// None
    /// </summary>
    None = 0,

    /// <summary>
    /// Account
    /// </summary>
    Account = 1 << 0,

    /// <summary>
    /// Builds
    /// </summary>
    Builds = 1 << 1,

    /// <summary>
    /// Characters
    /// </summary>
    Characters = 1 << 2,

    /// <summary>
    /// Guilds
    /// </summary>
    Guilds = 1 << 3,

    /// <summary>
    /// Inventories
    /// </summary>
    Inventories = 1 << 4,

    /// <summary>
    /// Progression
    /// </summary>
    Progression = 1 << 5,

    /// <summary>
    /// Player vs Player
    /// </summary>
    PvP = 1 << 6,

    /// <summary>
    /// Trading post
    /// </summary>
    TradingPost = 1 << 7,

    /// <summary>
    /// Unlocks
    /// </summary>
    Unlocks = 1 << 8,

    /// <summary>
    /// Wallet
    /// </summary>
    Wallet = 1 << 9,
}