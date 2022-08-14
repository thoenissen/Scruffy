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
    [ChoiceDisplay("Nightmare (98)")]
    Nightmare,
    [ChoiceDisplay("Shattered Observatory (99)")]
    ShatteredObservatory,
    [ChoiceDisplay("Sunqua Peak (100)")]
    SunquaPeak,

    // Strikes
    [ChoiceDisplay("IBS Strikes")]
    IBSStrikes,
    [ChoiceDisplay("EoD Strikes")]
    EoDStrikes,

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
        switch (value)
        {
            case DpsReportGroup.Nightmare:
            case DpsReportGroup.ShatteredObservatory:
            case DpsReportGroup.SunquaPeak:
                return DpsReportType.Fractal;
            case DpsReportGroup.IBSStrikes:
            case DpsReportGroup.EoDStrikes:
                return DpsReportType.Strike;
            case DpsReportGroup.SpritVale:
            case DpsReportGroup.SalvationPass:
            case DpsReportGroup.StrongholdOfTheFaithful:
            case DpsReportGroup.BastionOfThePenitent:
            case DpsReportGroup.HallOfChains:
            case DpsReportGroup.MythwrightGambit:
            case DpsReportGroup.TheKeyOfAhdashim:
                return DpsReportType.Raid;
            case DpsReportGroup.TrainingArea:
            default:
                {
                    return DpsReportType.Other;
                }
        }
    }

    /// <summary>
    /// Returns a proper text for every DPS report group
    /// </summary>
    /// <param name="value">The value to get the text for</param>
    /// <returns>A proper text for the given report group</returns>
    public static string AsText(this DpsReportGroup value)
    {
        switch (value)
        {
            case DpsReportGroup.Nightmare:
                return "Nightmare (98)";
            case DpsReportGroup.ShatteredObservatory:
                return "Shattered Observatory (99)";
            case DpsReportGroup.SunquaPeak:
                return "Sunqua Peak (100)";
            case DpsReportGroup.IBSStrikes:
                return "IBS Strikes";
            case DpsReportGroup.EoDStrikes:
                return "EoD Strikes";
            case DpsReportGroup.TrainingArea:
                return "Special Forces Training Area";
            case DpsReportGroup.SpritVale:
                return "W1 - Spirit Vale";
            case DpsReportGroup.SalvationPass:
                return "W2 - Salvation Pass";
            case DpsReportGroup.StrongholdOfTheFaithful:
                return "W3 - Stronghold of the Faithful";
            case DpsReportGroup.BastionOfThePenitent:
                return "W4 - Bastion of the Penitent";
            case DpsReportGroup.HallOfChains:
                return "W5 - Hall of Chains";
            case DpsReportGroup.MythwrightGambit:
                return "W6 - Mythwright Gambit";
            case DpsReportGroup.TheKeyOfAhdashim:
                return "W7 - The Key of Ahdashim";
            default:
                {
                    return "Other";
                }
        }
    }
}