using Discord.Interactions;

namespace Scruffy.Data.Enumerations.DpsReport;

/// <summary>
/// DPS report group
/// </summary>
public enum DpsReportGroupLegacy
{
    [Hide]
    Unknown,

    // Fractals
    [ChoiceDisplay("Nightmare")]
    Nightmare,
    [ChoiceDisplay("Shattered Observatory")]
    ShatteredObservatory,
    [ChoiceDisplay("Sunqua Peak")]
    SunquaPeak,
    [ChoiceDisplay("Silent Surf")]
    SilentSurf,
    [ChoiceDisplay("Lonely Tower")]
    LonelyTower,
    [ChoiceDisplay("Kinfall")]
    Kinfall,

    // Strikes
    [ChoiceDisplay("IBS Strikes")]
    IBSStrikes,
    [ChoiceDisplay("EoD Strikes")]
    EoDStrikes,
    [ChoiceDisplay("SotO Strikes")]
    SotOStrikes,

    // Raids
    [ChoiceDisplay("Special Forces Training Area")]
    TrainingArea,
    [ChoiceDisplay("W1 - Spirit Vale")]
    SpiritVale,
    [ChoiceDisplay("W2 - Salvation Pass")]
    SalvationPass,
    [ChoiceDisplay("W3 - Stronghold of the Faithful")]
    StrongholdOfTheFaithful,
    [ChoiceDisplay("W4 - Bastion of the Penitent")]
    BastionOfThePenitent,
    [ChoiceDisplay("W5 - Hall of Chains")]
    HallOfChains,
    [ChoiceDisplay("W6 - Mythwright Gambit")]
    MythwrightGambit,
    [ChoiceDisplay("W7 - The Key of Ahdashim")]
    TheKeyOfAhdashim,
    [ChoiceDisplay("W8 - Mount Balrior")]
    MountBalrior
}

/// <summary>
/// Extension methods for DPS report group
/// </summary>
public static class DpsReportGroupExtensions
{
    /// <summary>
    /// Returns the sort value for a given report group
    /// </summary>
    /// <param name="value">The value to get the sort value for</param>
    /// <returns>The sort value for the given report group</returns>
    public static int GetSortValue(this DpsReportGroupLegacy value)
    {
        return (int)value * 100;
    }

    /// <summary>
    /// Returns the report type for a given report group
    /// </summary>
    /// <param name="value">The value to get the type for</param>
    /// <returns>The report type for the given report group</returns>
    public static DpsReportType GetReportType(this DpsReportGroupLegacy value)
    {
        return value switch
               {
                   DpsReportGroupLegacy.Nightmare
                   or DpsReportGroupLegacy.ShatteredObservatory
                   or DpsReportGroupLegacy.SunquaPeak
                   or DpsReportGroupLegacy.SilentSurf
                   or DpsReportGroupLegacy.LonelyTower
                   or DpsReportGroupLegacy.Kinfall => DpsReportType.Fractal,

                   DpsReportGroupLegacy.IBSStrikes
                   or DpsReportGroupLegacy.EoDStrikes
                   or DpsReportGroupLegacy.SotOStrikes => DpsReportType.Strike,

                   DpsReportGroupLegacy.SpiritVale
                   or DpsReportGroupLegacy.SalvationPass
                   or DpsReportGroupLegacy.StrongholdOfTheFaithful
                   or DpsReportGroupLegacy.BastionOfThePenitent
                   or DpsReportGroupLegacy.HallOfChains
                   or DpsReportGroupLegacy.MythwrightGambit
                   or DpsReportGroupLegacy.TheKeyOfAhdashim
                   or DpsReportGroupLegacy.MountBalrior => DpsReportType.Raid,

                   _ => DpsReportType.Other,
               };
    }

    /// <summary>
    /// Returns a proper text for every DPS report group
    /// </summary>
    /// <param name="value">The value to get the text for</param>
    /// <returns>A proper text for the given report group</returns>
    public static string AsText(this DpsReportGroupLegacy value)
    {
        return value switch
               {
                   DpsReportGroupLegacy.Nightmare => "Nightmare",
                   DpsReportGroupLegacy.ShatteredObservatory => "Shattered Observatory",
                   DpsReportGroupLegacy.SunquaPeak => "Sunqua Peak",
                   DpsReportGroupLegacy.SilentSurf => "Silent Surf",
                   DpsReportGroupLegacy.LonelyTower => "Lonely Tower",
                   DpsReportGroupLegacy.Kinfall => "Kinfall",
                   DpsReportGroupLegacy.IBSStrikes => "IBS Strikes",
                   DpsReportGroupLegacy.EoDStrikes => "EoD Strikes",
                   DpsReportGroupLegacy.SotOStrikes => "SotO Strikes",
                   DpsReportGroupLegacy.TrainingArea => "Special Forces Training Area",
                   DpsReportGroupLegacy.SpiritVale => "W1 - Spirit Vale",
                   DpsReportGroupLegacy.SalvationPass => "W2 - Salvation Pass",
                   DpsReportGroupLegacy.StrongholdOfTheFaithful => "W3 - Stronghold of the Faithful",
                   DpsReportGroupLegacy.BastionOfThePenitent => "W4 - Bastion of the Penitent",
                   DpsReportGroupLegacy.HallOfChains => "W5 - Hall of Chains",
                   DpsReportGroupLegacy.MythwrightGambit => "W6 - Mythwright Gambit",
                   DpsReportGroupLegacy.TheKeyOfAhdashim => "W7 - The Key of Ahdashim",
                   DpsReportGroupLegacy.MountBalrior => "W8 - Mount Balrior",
                   _ => "Other"
               };
    }
}