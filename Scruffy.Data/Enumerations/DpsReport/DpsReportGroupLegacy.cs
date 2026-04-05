using Discord.Interactions;

namespace Scruffy.Data.Enumerations.DpsReport;

/// <summary>
/// DPS report group
/// </summary>
public enum DpsReportGroupLegacy
{
    /// <summary>
    /// Unknown report group
    /// </summary>
    [Hide]
    Unknown,

    // Fractals

    /// <summary>
    /// Nightmare fractal
    /// </summary>
    [ChoiceDisplay("Nightmare")]
    Nightmare,

    /// <summary>
    /// Shattered Observatory fractal
    /// </summary>
    [ChoiceDisplay("Shattered Observatory")]
    ShatteredObservatory,

    /// <summary>
    /// Sunqua Peak fractal
    /// </summary>
    [ChoiceDisplay("Sunqua Peak")]
    SunquaPeak,

    /// <summary>
    /// Silent Surf fractal
    /// </summary>
    [ChoiceDisplay("Silent Surf")]
    SilentSurf,

    /// <summary>
    /// Lonely Tower fractal
    /// </summary>
    [ChoiceDisplay("Lonely Tower")]
    LonelyTower,

    /// <summary>
    /// Kinfall fractal
    /// </summary>
    [ChoiceDisplay("Kinfall")]
    Kinfall,

    // Strikes

    /// <summary>
    /// IBS strikes
    /// </summary>
    [ChoiceDisplay("IBS Strikes")]
    IBSStrikes,

    /// <summary>
    /// EoD strikes
    /// </summary>
    [ChoiceDisplay("EoD Strikes")]
    EoDStrikes,

    /// <summary>
    /// SotO strikes
    /// </summary>
    [ChoiceDisplay("SotO Strikes")]
    SotOStrikes,

    // Raids

    /// <summary>
    /// Special Forces Training Area raid
    /// </summary>
    [ChoiceDisplay("Special Forces Training Area")]
    TrainingArea,

    /// <summary>
    /// W1 - Spirit Vale raid
    /// </summary>
    [ChoiceDisplay("W1 - Spirit Vale")]
    SpiritVale,

    /// <summary>
    /// W2 - Salvation Pass raid
    /// </summary>
    [ChoiceDisplay("W2 - Salvation Pass")]
    SalvationPass,

    /// <summary>
    /// W3 - Stronghold of the Faithful raid
    /// </summary>
    [ChoiceDisplay("W3 - Stronghold of the Faithful")]
    StrongholdOfTheFaithful,

    /// <summary>
    /// W4 - Bastion of the Penitent raid
    /// </summary>
    [ChoiceDisplay("W4 - Bastion of the Penitent")]
    BastionOfThePenitent,

    /// <summary>
    /// W5 - Hall of Chains raid
    /// </summary>
    [ChoiceDisplay("W5 - Hall of Chains")]
    HallOfChains,

    /// <summary>
    /// W6 - Mythwright Gambit raid
    /// </summary>
    [ChoiceDisplay("W6 - Mythwright Gambit")]
    MythwrightGambit,

    /// <summary>
    /// W7 - The Key of Ahdashim raid
    /// </summary>
    [ChoiceDisplay("W7 - The Key of Ahdashim")]
    TheKeyOfAhdashim,

    /// <summary>
    /// W8 - Mount Balrior raid
    /// </summary>
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