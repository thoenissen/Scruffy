using System;

namespace Scruffy.WebApp.DTOs.Raid;

/// <summary>
/// Special raid roles
/// </summary>
[Flags]
public enum RaidSpecialRole
{
    /// <summary>
    /// None
    /// </summary>
    None = 0,

    /// <summary>
    /// Hand kiter
    /// </summary>
    HandKiter,

    /// <summary>
    /// Soulless horror pusher
    /// </summary>
    SoullessHorrorPusher,

    /// <summary>
    /// Quadim 1 - Kiter
    /// </summary>
    Quadim1Kiter,

    /// <summary>
    /// Quadim 1 - Kiter
    /// </summary>
    Quadim2Kiter
}