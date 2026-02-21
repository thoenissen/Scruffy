using Scruffy.Data.Enumerations.DpsReport;

namespace Scruffy.Data.Extensions;

/// <summary>
/// Display string extensions for DPS report enumerations
/// </summary>
public static class DpsReportDisplayExtensions
{
    /// <summary>
    /// Returns a user-friendly display string for the given group
    /// </summary>
    /// <param name="value">The group value</param>
    /// <returns>A display string suitable for user output</returns>
    public static string ToDisplayString(this DpsReportGroup value)
    {
        return value switch
               {
                   DpsReportGroup.TrainingArea => "Training Area",
                   DpsReportGroup.Fractals => "Fractals",
                   DpsReportGroup.CoreGame => "Core Game",
                   DpsReportGroup.HeartsOfThorns => "Heart of Thorns",
                   DpsReportGroup.PathOfFire => "Path of Fire",
                   DpsReportGroup.IcebroodSaga => "Icebrood Saga",
                   DpsReportGroup.EndOfDragons => "End of Dragons",
                   DpsReportGroup.SecretsOfTheObscure => "Secrets of the Obscure",
                   DpsReportGroup.JanthirWilds => "Janthir Wilds",
                   DpsReportGroup.VisionsOfEternity => "Visions of Eternity",
                   _ => value.ToString()
               };
    }

    /// <summary>
    /// Returns a user-friendly display string for the given sub group
    /// </summary>
    /// <param name="value">The sub group value</param>
    /// <returns>A display string suitable for user output</returns>
    public static string ToDisplayString(this DpsReportSubGroup value)
    {
        return value switch
               {
                   DpsReportSubGroup.NotDefined => string.Empty,
                   DpsReportSubGroup.Nightmare => "Nightmare",
                   DpsReportSubGroup.ShatteredObservatory => "Shattered Observatory",
                   DpsReportSubGroup.SunquaPeak => "Sunqua Peak",
                   DpsReportSubGroup.SilentSurf => "Silent Surf",
                   DpsReportSubGroup.LonelyTower => "Lonely Tower",
                   DpsReportSubGroup.Kinfall => "Kinfall",
                   DpsReportSubGroup.OldLionsCourt => "Old Lion's Court",
                   DpsReportSubGroup.SpiritVale => "Spirit Vale",
                   DpsReportSubGroup.SalvationPass => "Salvation Pass",
                   DpsReportSubGroup.StrongholdOfTheFaithful => "Stronghold of the Faithful",
                   DpsReportSubGroup.BastionOfThePenitent => "Bastion of the Penitent",
                   DpsReportSubGroup.HallOfChains => "Hall of Chains",
                   DpsReportSubGroup.MythwrightGambit => "Mythwright Gambit",
                   DpsReportSubGroup.TheKeyOfAhdashim => "The Key of Ahdashim",
                   DpsReportSubGroup.ShiverpeaksPass => "Shiverpeaks Pass",
                   DpsReportSubGroup.SanctuumArena => "Sanctum Arena",
                   DpsReportSubGroup.WhisperingDepths => "Whispering Depths",
                   DpsReportSubGroup.AetherbladHideout => "Aetherblade Hideout",
                   DpsReportSubGroup.XunlaiJadeJunkyard => "Xunlai Jade Junkyard",
                   DpsReportSubGroup.KainengOverlook => "Kaineng Overlook",
                   DpsReportSubGroup.HarvestTemple => "Harvest Temple",
                   DpsReportSubGroup.CosmicObservatory => "Cosmic Observatory",
                   DpsReportSubGroup.TempleOfFebe => "Temple of Febe",
                   DpsReportSubGroup.MountBalrior => "Mount Balrior",
                   DpsReportSubGroup.GuardiansGlade => "Guardian's Glade",
                   _ => value.ToString()
               };
    }

    /// <summary>
    /// Returns a user-friendly display string for the given encounter target
    /// </summary>
    /// <param name="value">The encounter target value</param>
    /// <returns>A display string suitable for user output</returns>
    public static string ToDisplayString(this DpsReportEncounterTarget value)
    {
        return value switch
               {
                   DpsReportEncounterTarget.TrainingsGolem => "Training Golem",
                   DpsReportEncounterTarget.MAMA => "MAMA",
                   DpsReportEncounterTarget.Siax => "Siax",
                   DpsReportEncounterTarget.Ensolyss => "Ensolyss",
                   DpsReportEncounterTarget.Skorvald => "Skorvald",
                   DpsReportEncounterTarget.Artsariiv => "Artsariiv",
                   DpsReportEncounterTarget.Arkk => "Arkk",
                   DpsReportEncounterTarget.AiKeeperOfThePeak => "Ai, Keeper of the Peak",
                   DpsReportEncounterTarget.Kanaxai => "Kanaxai",
                   DpsReportEncounterTarget.CerusLonelyTower => "Cerus",
                   DpsReportEncounterTarget.DeimosLonelyTower => "Deimos",
                   DpsReportEncounterTarget.EparchLonelyTower => "Eparch",
                   DpsReportEncounterTarget.WhisperingShadow => "Whispering Shadow",
                   DpsReportEncounterTarget.IcebroodConstruct => "Icebrood Construct",
                   DpsReportEncounterTarget.VoiceAndClawOfTheFallen => "Voice and Claw of the Fallen",
                   DpsReportEncounterTarget.FraenirOfJormag => "Fraenir of Jormag",
                   DpsReportEncounterTarget.WhisperOfJormag => "Whisper of Jormag",
                   DpsReportEncounterTarget.VariniaStormsounder => "Varinia Stormsounder",
                   DpsReportEncounterTarget.Boneskinner => "Boneskinner",
                   DpsReportEncounterTarget.MaiTrin => "Mai Trin",
                   DpsReportEncounterTarget.Ankka => "Ankka",
                   DpsReportEncounterTarget.MinisterLi => "Minister Li",
                   DpsReportEncounterTarget.VoidAmalgamate => "Void Amalgamate",
                   DpsReportEncounterTarget.PrototypeVermilion => "Prototype Vermilion",
                   DpsReportEncounterTarget.Dagda => "Dagda",
                   DpsReportEncounterTarget.Cerus => "Cerus",
                   DpsReportEncounterTarget.ValeGuardian => "Vale Guardian",
                   DpsReportEncounterTarget.Gorseval => "Gorseval",
                   DpsReportEncounterTarget.Sabetha => "Sabetha",
                   DpsReportEncounterTarget.Slothasor => "Slothasor",
                   DpsReportEncounterTarget.BanditTrio => "Bandit Trio",
                   DpsReportEncounterTarget.Matthias => "Matthias",
                   DpsReportEncounterTarget.McLeodTheSilent => "McLeod the Silent",
                   DpsReportEncounterTarget.KeepConstruct => "Keep Construct",
                   DpsReportEncounterTarget.HauntingStatue => "Twisted Castle",
                   DpsReportEncounterTarget.Xera => "Xera",
                   DpsReportEncounterTarget.Cairn => "Cairn",
                   DpsReportEncounterTarget.MursaatOverseer => "Mursaat Overseer",
                   DpsReportEncounterTarget.Samarog => "Samarog",
                   DpsReportEncounterTarget.Deimos => "Deimos",
                   DpsReportEncounterTarget.SoullessHorror => "Soulless Horror",
                   DpsReportEncounterTarget.Desmina => "Desmina",
                   DpsReportEncounterTarget.EyeOfJudgement => "Eye of Judgement",
                   DpsReportEncounterTarget.EyeOfFate => "Eye of Fate",
                   DpsReportEncounterTarget.EaterOfSouls => "Eater of Souls",
                   DpsReportEncounterTarget.BrokenKing => "Broken King",
                   DpsReportEncounterTarget.Dhuum => "Dhuum",
                   DpsReportEncounterTarget.ConjuredAmalgamate => "Conjured Amalgamate",
                   DpsReportEncounterTarget.TwinLargos => "Twin Largos",
                   DpsReportEncounterTarget.Qadim => "Qadim",
                   DpsReportEncounterTarget.Adina => "Adina",
                   DpsReportEncounterTarget.Sabir => "Sabir",
                   DpsReportEncounterTarget.QadimThePeerless => "Qadim the Peerless",
                   DpsReportEncounterTarget.Decima => "Decima",
                   DpsReportEncounterTarget.Greer => "Greer",
                   DpsReportEncounterTarget.Ura => "Ura",
                   _ => value.ToString()
               };
    }
}