namespace Scruffy.Data.Enumerations.DpsReport;

/// <summary>
/// Extension methods for DPS report group
/// </summary>
public static class DpsReportGroupExtensions
{
    #region Methods

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

    #endregion // Methods
}