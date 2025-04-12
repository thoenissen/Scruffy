using System;

namespace Scruffy.WebApp.DTOs.Raid;

/// <summary>
/// Represents statistics for a specific raid day.
/// </summary>
public class RaidDayStatisticsDTO
{
    /// <summary>
    /// Day
    /// </summary>
    public DayOfWeek Day { get; set; }

    /// <summary>
    /// Damage dealers
    /// </summary>
    public double DamageDealer { get; set; }

    /// <summary>
    /// Alacrity damage dealers
    /// </summary>
    public double AlacrityDamageDealer { get; set; }

    /// <summary>
    /// Quickness damage dealers
    /// </summary>
    public double QuicknessDamageDealer { get; set; }

    /// <summary>
    /// Alacrity healers
    /// </summary>
    public double AlacrityHealer { get; set; }

    /// <summary>
    /// Quickness healers
    /// </summary>
    public double QuicknessHealer { get; set; }

    /// <summary>
    /// Alacrity tank healers
    /// </summary>
    public double AlacrityTankHealer { get; set; }

    /// <summary>
    /// Quickness tank healers
    /// </summary>
    public double QuicknessTankHealer { get; set; }

    /// <summary>
    /// Hand kiters
    /// </summary>
    public double HandKiter { get; set; }

    /// <summary>
    /// Soulless horror pushers
    /// </summary>
    public double SoullessHorrorPusher { get; set; }

    /// <summary>
    /// Quadim 1 kiters
    /// </summary>
    public double Quadim1Kiter { get; set; }

    /// <summary>
    /// Quadim 2 kiters
    /// </summary>
    public double Quadim2Kiter { get; set; }
}