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
    HandKiter = 1 << 0,

    /// <summary>
    /// Soulless horror pusher
    /// </summary>
    SoullessHorrorPusher = 1 << 1,

    /// <summary>
    /// Quadim 1 - Kiter
    /// </summary>
    Quadim1Kiter = 1 << 2,

    /// <summary>
    /// Quadim 1 - Kiter
    /// </summary>
    Quadim2Kiter = 1 << 3,
}