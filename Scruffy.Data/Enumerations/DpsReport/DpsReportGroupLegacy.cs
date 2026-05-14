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