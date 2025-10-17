using Discord.Interactions;

namespace Scruffy.Data.Enumerations.DpsReport;

/// <summary>
/// DPS report group
/// </summary>
public enum DpsReportGroup
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
    SpritVale,
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
    public static int GetSortValue(this DpsReportGroup value)
    {
        return (int)value * 100;
    }

    /// <summary>
    /// Returns the report type for a given report group
    /// </summary>
    /// <param name="value">The value to get the type for</param>
    /// <returns>The report type for the given report group</returns>
    public static DpsReportType GetReportType(this DpsReportGroup value)
    {
        return value switch
               {
                   DpsReportGroup.Nightmare
                   or DpsReportGroup.ShatteredObservatory
                   or DpsReportGroup.SunquaPeak
                   or DpsReportGroup.SilentSurf
                   or DpsReportGroup.LonelyTower
                   or DpsReportGroup.Kinfall => DpsReportType.Fractal,

                   DpsReportGroup.IBSStrikes
                   or DpsReportGroup.EoDStrikes
                   or DpsReportGroup.SotOStrikes => DpsReportType.Strike,

                   DpsReportGroup.SpritVale
                   or DpsReportGroup.SalvationPass
                   or DpsReportGroup.StrongholdOfTheFaithful
                   or DpsReportGroup.BastionOfThePenitent
                   or DpsReportGroup.HallOfChains
                   or DpsReportGroup.MythwrightGambit
                   or DpsReportGroup.TheKeyOfAhdashim
                   or DpsReportGroup.MountBalrior => DpsReportType.Raid,

                   _ => DpsReportType.Other,
               };
    }

    /// <summary>
    /// Returns a proper text for every DPS report group
    /// </summary>
    /// <param name="value">The value to get the text for</param>
    /// <returns>A proper text for the given report group</returns>
    public static string AsText(this DpsReportGroup value)
    {
        return value switch
               {
                   DpsReportGroup.Nightmare => "Nightmare",
                   DpsReportGroup.ShatteredObservatory => "Shattered Observatory",
                   DpsReportGroup.SunquaPeak => "Sunqua Peak",
                   DpsReportGroup.SilentSurf => "Silent Surf",
                   DpsReportGroup.LonelyTower => "Lonely Tower",
                   DpsReportGroup.Kinfall => "Kinfall",
                   DpsReportGroup.IBSStrikes => "IBS Strikes",
                   DpsReportGroup.EoDStrikes => "EoD Strikes",
                   DpsReportGroup.SotOStrikes => "SotO Strikes",
                   DpsReportGroup.TrainingArea => "Special Forces Training Area",
                   DpsReportGroup.SpritVale => "W1 - Spirit Vale",
                   DpsReportGroup.SalvationPass => "W2 - Salvation Pass",
                   DpsReportGroup.StrongholdOfTheFaithful => "W3 - Stronghold of the Faithful",
                   DpsReportGroup.BastionOfThePenitent => "W4 - Bastion of the Penitent",
                   DpsReportGroup.HallOfChains => "W5 - Hall of Chains",
                   DpsReportGroup.MythwrightGambit => "W6 - Mythwright Gambit",
                   DpsReportGroup.TheKeyOfAhdashim => "W7 - The Key of Ahdashim",
                   DpsReportGroup.MountBalrior => "W8 - Mount Balrior",
                   _ => "Other"
               };
    }
}