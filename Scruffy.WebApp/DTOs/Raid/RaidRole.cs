using System;

namespace Scruffy.WebApp.DTOs.Raid;

/// <summary>
/// Raid roles
/// </summary>
[Flags]
public enum RaidRole
{
    /// <summary>
    /// DPS
    /// </summary>
    DamageDealer = 1 << 1,

    /// <summary>
    /// Alacrity - DPS
    /// </summary>
    AlacrityDamageDealer = 1 << 2,

    /// <summary>
    /// Quickness - DPS
    /// </summary>
    QuicknessDamageDealer = 1 << 3,

    /// <summary>
    /// Alacrity - Healer
    /// </summary>
    AlacrityHealer = 1 << 4,

    /// <summary>
    /// Quickness - Healer
    /// </summary>
    QuicknessHealer = 1 << 5,

    /// <summary>
    /// Alacrity - Tank - Healer
    /// </summary>
    AlacrityTankHealer = 1 << 6,

    /// <summary>
    /// Quickness - Tank - Healer
    /// </summary>
    QuicknessTankHealer = 1 << 7,
}