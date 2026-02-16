using GW2EIEvtcParser;

using Scruffy.Data.Enumerations.DpsReport;
using Scruffy.Services.GuildWars2.DpsReports;

namespace Scruffy.Services.GuildWars2;

/// <summary>
/// Analyzing dps.report reports
/// </summary>
internal static class DpsReportAnalyzer
{
    /// <summary>
    /// Get boss icon ID
    /// </summary>
    /// <param name="bossId">Boss ID</param>
    /// <returns>Icon ID</returns>
    public static ulong GetRaidBossIconId(int bossId)
    {
        var targetId = SpeciesIDs.GetTargetID(bossId);

        return targetId switch
               {
                   // Wing 1
                   SpeciesIDs.TargetID.ValeGuardian => 848910035747864576,
                   SpeciesIDs.TargetID.Gorseval => 848908993538949131,
                   SpeciesIDs.TargetID.Sabetha => 848909543915651072,

                   // Wing 2
                   SpeciesIDs.TargetID.Slothasor => 848909627982610482,
                   SpeciesIDs.TargetID.Berg => 848909882115358720,
                   SpeciesIDs.TargetID.Zane => 848909882115358720,
                   SpeciesIDs.TargetID.Narella => 848909882115358720,
                   SpeciesIDs.TargetID.Matthias => 848909162821845043,

                   // Wing 3
                   SpeciesIDs.TargetID.McLeodTheSilent => 743938372195844117,
                   SpeciesIDs.TargetID.KeepConstruct => 848909049599885322,
                   SpeciesIDs.TargetID.HauntingStatue => 848909953112473622,
                   SpeciesIDs.TargetID.Xera => 848910090370940949,

                   // Wing 4
                   SpeciesIDs.TargetID.Cairn => 848908521680142359,
                   SpeciesIDs.TargetID.MursaatOverseer => 848909340827713557,
                   SpeciesIDs.TargetID.Samarog => 848909587938803762,
                   SpeciesIDs.TargetID.Deimos => 848908773996101642,

                   // Wing 5
                   SpeciesIDs.TargetID.SoullessHorror => 848911345964679188,
                   SpeciesIDs.TargetID.Desmina => 743940484455596064,
                   SpeciesIDs.TargetID.EyeOfJudgement => 848909739509547058,
                   SpeciesIDs.TargetID.EyeOfFate => 848909739509547058,
                   SpeciesIDs.TargetID.EaterOfSouls => 848908876039585822,
                   SpeciesIDs.TargetID.BrokenKing => 848908317832773692,
                   SpeciesIDs.TargetID.Dhuum => 848908828866379777,

                   // Wing 6
                   SpeciesIDs.TargetID.ConjuredAmalgamate => 848908712692547614,
                   SpeciesIDs.TargetID.Nikare => 848909098619895808,
                   SpeciesIDs.TargetID.Kenut => 848909098619895808,
                   SpeciesIDs.TargetID.Qadim => 848909410691973140,

                   // Wing 7
                   SpeciesIDs.TargetID.Adina => 848908580749049866,
                   SpeciesIDs.TargetID.Sabir => 848908653637533736,
                   SpeciesIDs.TargetID.QadimThePeerless => 848909465553207296,
                   _ => 0ul
               };
    }

    /// <summary>
    /// Determines the report group of a given boss
    /// </summary>
    /// <param name="bossId">The ID of the boss to determine the group</param>
    /// <returns>report group of the boss</returns>
    public static DpsReportGroup GetReportGroupByBossId(int bossId)
    {
        var targetId = SpeciesIDs.GetTargetID(bossId);

        return targetId switch
               {
                   SpeciesIDs.TargetID.MAMA
                   or SpeciesIDs.TargetID.Siax
                   or SpeciesIDs.TargetID.Ensolyss => DpsReportGroup.Nightmare,

                   SpeciesIDs.TargetID.Skorvald
                   or SpeciesIDs.TargetID.Artsariiv
                   or SpeciesIDs.TargetID.Arkk => DpsReportGroup.ShatteredObservatory,

                   SpeciesIDs.TargetID.AiKeeperOfThePeak => DpsReportGroup.SunquaPeak,

                   SpeciesIDs.TargetID.KanaxaiScytheOfHouseAurkusNM
                   or SpeciesIDs.TargetID.KanaxaiScytheOfHouseAurkusCM => DpsReportGroup.SilentSurf,

                   SpeciesIDs.TargetID.CerusLonelyTower
                   or SpeciesIDs.TargetID.DeimosLonelyTower
                   or SpeciesIDs.TargetID.EparchLonelyTower => DpsReportGroup.LonelyTower,

                   SpeciesIDs.TargetID.WhisperingShadow => DpsReportGroup.Kinfall,

                   SpeciesIDs.TargetID.IcebroodConstruct
                   or SpeciesIDs.TargetID.VoiceOfTheFallen
                   or SpeciesIDs.TargetID.ClawOfTheFallen
                   or SpeciesIDs.TargetID.FraenirOfJormag
                   or SpeciesIDs.TargetID.IcebroodConstructFraenir
                   or SpeciesIDs.TargetID.WhisperOfJormag
                   or SpeciesIDs.TargetID.VariniaStormsounder
                   or SpeciesIDs.TargetID.Boneskinner => DpsReportGroup.IBSStrikes,

                   SpeciesIDs.TargetID.MaiTrinRaid
                   or SpeciesIDs.TargetID.EchoOfScarletBriarNM
                   or SpeciesIDs.TargetID.EchoOfScarletBriarCM
                   or SpeciesIDs.TargetID.Ankka
                   or SpeciesIDs.TargetID.MinisterLi
                   or SpeciesIDs.TargetID.MinisterLiCM
                   or SpeciesIDs.TargetID.GadgetTheDragonVoid1
                   or SpeciesIDs.TargetID.GadgetTheDragonVoid2
                   or SpeciesIDs.TargetID.VoidAmalgamate
                   or SpeciesIDs.TargetID.PrototypeVermilionCM => DpsReportGroup.EoDStrikes,

                   SpeciesIDs.TargetID.Dagda
                   or SpeciesIDs.TargetID.Cerus => DpsReportGroup.SotOStrikes,

                   SpeciesIDs.TargetID.MassiveGolem10M
                   or SpeciesIDs.TargetID.MassiveGolem4M
                   or SpeciesIDs.TargetID.MassiveGolem1M
                   or SpeciesIDs.TargetID.VitalGolem
                   or SpeciesIDs.TargetID.AvgGolem
                   or SpeciesIDs.TargetID.StdGolem
                   or SpeciesIDs.TargetID.LGolem
                   or SpeciesIDs.TargetID.MedGolem
                   or SpeciesIDs.TargetID.ConditionGolem
                   or SpeciesIDs.TargetID.PowerGolem => DpsReportGroup.TrainingArea,

                   SpeciesIDs.TargetID.ValeGuardian
                   or SpeciesIDs.TargetID.Gorseval
                   or SpeciesIDs.TargetID.Sabetha => DpsReportGroup.SpiritVale,

                   SpeciesIDs.TargetID.Slothasor
                   or SpeciesIDs.TargetID.Berg
                   or SpeciesIDs.TargetID.Zane
                   or SpeciesIDs.TargetID.Narella
                   or SpeciesIDs.TargetID.Matthias => DpsReportGroup.SalvationPass,

                   SpeciesIDs.TargetID.McLeodTheSilent
                   or SpeciesIDs.TargetID.KeepConstruct
                   or SpeciesIDs.TargetID.HauntingStatue
                   or SpeciesIDs.TargetID.Xera => DpsReportGroup.StrongholdOfTheFaithful,

                   SpeciesIDs.TargetID.Cairn
                   or SpeciesIDs.TargetID.MursaatOverseer
                   or SpeciesIDs.TargetID.Samarog
                   or SpeciesIDs.TargetID.Deimos => DpsReportGroup.BastionOfThePenitent,

                   SpeciesIDs.TargetID.SoullessHorror
                   or SpeciesIDs.TargetID.Desmina
                   or SpeciesIDs.TargetID.BrokenKing
                   or SpeciesIDs.TargetID.EaterOfSouls
                   or SpeciesIDs.TargetID.EyeOfJudgement
                   or SpeciesIDs.TargetID.EyeOfFate
                   or SpeciesIDs.TargetID.Dhuum => DpsReportGroup.HallOfChains,

                   SpeciesIDs.TargetID.ConjuredAmalgamate
                   or SpeciesIDs.TargetID.CARightArm
                   or SpeciesIDs.TargetID.CALeftArm
                   or SpeciesIDs.TargetID.Nikare
                   or SpeciesIDs.TargetID.Kenut
                   or SpeciesIDs.TargetID.Qadim => DpsReportGroup.MythwrightGambit,

                   SpeciesIDs.TargetID.Adina
                   or SpeciesIDs.TargetID.Sabir
                   or SpeciesIDs.TargetID.QadimThePeerless => DpsReportGroup.TheKeyOfAhdashim,

                   SpeciesIDs.TargetID.Decima
                   or SpeciesIDs.TargetID.Greer
                   or SpeciesIDs.TargetID.Ura => DpsReportGroup.MountBalrior,

                   _ => DpsReportGroup.Unknown
               };
    }

    /// <summary>
    /// Get encounter order of the given boss ID
    /// </summary>
    /// <param name="bossId">Boss ID</param>
    /// <returns>Order</returns>
    public static int GetBossOrder(int bossId)
    {
        var targetId = SpeciesIDs.GetTargetID(bossId);

        return targetId switch
               {
                   // Training Area
                   SpeciesIDs.TargetID.MassiveGolem1M => 1,
                   SpeciesIDs.TargetID.MassiveGolem4M => 2,
                   SpeciesIDs.TargetID.MassiveGolem10M => 3,
                   SpeciesIDs.TargetID.VitalGolem => 4,
                   SpeciesIDs.TargetID.AvgGolem => 5,
                   SpeciesIDs.TargetID.StdGolem => 6,
                   SpeciesIDs.TargetID.MedGolem => 7,
                   SpeciesIDs.TargetID.LGolem => 8,
                   SpeciesIDs.TargetID.ConditionGolem => 9,
                   SpeciesIDs.TargetID.PowerGolem => 10,

                   // Nightmare
                   SpeciesIDs.TargetID.MAMA => 1,
                   SpeciesIDs.TargetID.Siax => 2,
                   SpeciesIDs.TargetID.Ensolyss => 3,

                   // Shattered Observatory
                   SpeciesIDs.TargetID.Skorvald => 1,
                   SpeciesIDs.TargetID.Artsariiv => 2,
                   SpeciesIDs.TargetID.Arkk => 3,

                   // Sunqua Peak
                   SpeciesIDs.TargetID.AiKeeperOfThePeak => 1,

                   // Silent Surf
                   SpeciesIDs.TargetID.KanaxaiScytheOfHouseAurkusNM => 1,
                   SpeciesIDs.TargetID.KanaxaiScytheOfHouseAurkusCM => 2,

                   // Lonely Tower
                   SpeciesIDs.TargetID.CerusLonelyTower => 1,
                   SpeciesIDs.TargetID.DeimosLonelyTower => 2,
                   SpeciesIDs.TargetID.EparchLonelyTower => 3,

                   // Kinfall
                   SpeciesIDs.TargetID.WhisperingShadow => 1,

                   // IBS Strikes
                   SpeciesIDs.TargetID.IcebroodConstruct => 1,
                   SpeciesIDs.TargetID.VoiceOfTheFallen => 2,
                   SpeciesIDs.TargetID.ClawOfTheFallen => 3,
                   SpeciesIDs.TargetID.FraenirOfJormag => 4,
                   SpeciesIDs.TargetID.IcebroodConstructFraenir => 5,
                   SpeciesIDs.TargetID.WhisperOfJormag => 6,
                   SpeciesIDs.TargetID.VariniaStormsounder => 7,
                   SpeciesIDs.TargetID.Boneskinner => 8,

                   // EoD Strikes
                   SpeciesIDs.TargetID.MaiTrinRaid => 1,
                   SpeciesIDs.TargetID.EchoOfScarletBriarNM => 2,
                   SpeciesIDs.TargetID.EchoOfScarletBriarCM => 3,
                   SpeciesIDs.TargetID.Ankka => 3,
                   SpeciesIDs.TargetID.MinisterLi => 4,
                   SpeciesIDs.TargetID.MinisterLiCM => 5,
                   SpeciesIDs.TargetID.GadgetTheDragonVoid1 => 6,
                   SpeciesIDs.TargetID.GadgetTheDragonVoid2 => 7,
                   SpeciesIDs.TargetID.VoidAmalgamate => 8,
                   SpeciesIDs.TargetID.PrototypeVermilionCM => 9,

                   // SotO Strikes
                   SpeciesIDs.TargetID.Dagda => 1,
                   SpeciesIDs.TargetID.Cerus => 2,

                   // Wing 1
                   SpeciesIDs.TargetID.ValeGuardian => 1,
                   SpeciesIDs.TargetID.Gorseval => 2,
                   SpeciesIDs.TargetID.Sabetha => 3,

                   // Wing 2
                   SpeciesIDs.TargetID.Slothasor => 1,
                   SpeciesIDs.TargetID.Berg => 2,
                   SpeciesIDs.TargetID.Zane => 3,
                   SpeciesIDs.TargetID.Narella => 4,
                   SpeciesIDs.TargetID.Matthias => 5,

                   // Wing 3
                   SpeciesIDs.TargetID.McLeodTheSilent => 1,
                   SpeciesIDs.TargetID.KeepConstruct => 2,
                   SpeciesIDs.TargetID.HauntingStatue => 3,
                   SpeciesIDs.TargetID.Xera => 4,

                   // Wing 4
                   SpeciesIDs.TargetID.Cairn => 1,
                   SpeciesIDs.TargetID.MursaatOverseer => 2,
                   SpeciesIDs.TargetID.Samarog => 3,
                   SpeciesIDs.TargetID.Deimos => 4,

                   // Wing 5
                   SpeciesIDs.TargetID.SoullessHorror => 1,
                   SpeciesIDs.TargetID.Desmina => 2,
                   SpeciesIDs.TargetID.EyeOfJudgement => 3,
                   SpeciesIDs.TargetID.EyeOfFate => 4,
                   SpeciesIDs.TargetID.EaterOfSouls => 5,
                   SpeciesIDs.TargetID.BrokenKing => 6,
                   SpeciesIDs.TargetID.Dhuum => 7,

                   // Wing 6
                   SpeciesIDs.TargetID.ConjuredAmalgamate => 1,
                   SpeciesIDs.TargetID.CARightArm => 2,
                   SpeciesIDs.TargetID.CALeftArm => 3,
                   SpeciesIDs.TargetID.Nikare => 4,
                   SpeciesIDs.TargetID.Kenut => 5,
                   SpeciesIDs.TargetID.Qadim => 6,

                   // Wing 7
                   SpeciesIDs.TargetID.Adina => 1,
                   SpeciesIDs.TargetID.Sabir => 2,
                   SpeciesIDs.TargetID.QadimThePeerless => 3,

                   // Wing 8
                   SpeciesIDs.TargetID.Decima => 1,
                   SpeciesIDs.TargetID.Greer => 2,
                   SpeciesIDs.TargetID.Ura => 3,

                   // Unknown
                   _ => 0
               };
    }

    /// <summary>
    /// Gets all encounters organized by expansion
    /// </summary>
    /// <returns>List of expansions with their encounters and bosses</returns>
    public static List<DpsReportExpansionEntry> GetEncounters()
    {
        return [
                   CreateCoreGame(),
                   CreateHeartsOfThorns(),
                   CreatePathOfFire(),
                   CreateIcebroodSaga(),
                   CreateEndOfDragons(),
                   CreateSecretsOfTheObscure(),
                   CreateJanthirWilds(),
                   CreateVisionsOfEternity()
               ];
    }

    /// <summary>
    /// Creates the Core Game expansion entry with encounters
    /// </summary>
    /// <returns>A configured DpsReportExpansionEntry for Core Game</returns>
    private static DpsReportExpansionEntry CreateCoreGame()
    {
        return new()
               {
                   ExpansionId = DpsReportExpansion.CoreGame,
                   Name = "Core Game",
                   Encounters =
                   [
                       new()
                       {
                           EncounterId = DpsReportEncounter.OldLionsCourt,
                           Name = "Old Lion's Court",
                           Bosses =
                           [
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.LGolem,
                                   Name = "Lion's Arch"
                               }
                           ]
                       }
                   ]
               };
    }

    /// <summary>
    /// Creates the Heart of Thorns expansion entry with encounters
    /// </summary>
    /// <returns>A configured DpsReportExpansionEntry for Heart of Thorns</returns>
    private static DpsReportExpansionEntry CreateHeartsOfThorns()
    {
        return new()
               {
                   ExpansionId = DpsReportExpansion.HeartsOfThorns,
                   Name = "Heart of Thorns",
                   Encounters =
                   [
                       new()
                       {
                           EncounterId = DpsReportEncounter.SpiritVale,
                           Name = "Spirit Vale",
                           Bosses =
                           [
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.ValeGuardian,
                                   Name = "Vale Guardian"
                               },
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.Gorseval,
                                   Name = "Gorseval"
                               },
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.Sabetha,
                                   Name = "Sabetha"
                               }
                           ]
                       },
                       new()
                       {
                           EncounterId = DpsReportEncounter.SalvationPass,
                           Name = "Salvation Pass",
                           Bosses =
                           [
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.Slothasor,
                                   Name = "Slothasor"
                               },
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.Berg,
                                   Name = "Bandit Trio"
                               },
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.Matthias,
                                   Name = "Matthias"
                               }
                           ]
                       },
                       new()
                       {
                           EncounterId = DpsReportEncounter.StrongholdOfTheFaithful,
                           Name = "Stronghold of the Faithful",
                           Bosses =
                           [
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.KeepConstruct,
                                   Name = "Keep Construct"
                               },
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.Xera,
                                   Name = "Xera"
                               }
                           ]
                       },
                       new()
                       {
                           EncounterId = DpsReportEncounter.BastionOfThePenitent,
                           Name = "Bastion of the Penitent",
                           Bosses =
                           [
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.Cairn,
                                   Name = "Cairn"
                               },
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.MursaatOverseer,
                                   Name = "Mursaat Overseer"
                               },
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.Samarog,
                                   Name = "Samarog"
                               },
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.Deimos,
                                   Name = "Deimos"
                               }
                           ]
                       }
                   ]
               };
    }

    /// <summary>
    /// Creates the Path of Fire expansion entry with encounters
    /// </summary>
    /// <returns>A configured DpsReportExpansionEntry for Path of Fire</returns>
    private static DpsReportExpansionEntry CreatePathOfFire()
    {
        return new()
               {
                   ExpansionId = DpsReportExpansion.PathOfFire,
                   Name = "Path of Fire",
                   Encounters =
                   [
                       new()
                       {
                           EncounterId = DpsReportEncounter.HallOfChains,
                           Name = "Hall of Chains",
                           Bosses =
                           [
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.SoullessHorror,
                                   Name = "Soulless Horror"
                               },
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.Dhuum,
                                   Name = "Voice in the Void (Dhuum)"
                               }
                           ]
                       },
                       new()
                       {
                           EncounterId = DpsReportEncounter.MythwrightGambit,
                           Name = "Mythwright Gambit",
                           Bosses =
                           [
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.ConjuredAmalgamate,
                                   Name = "Conjured Amalgamate"
                               },
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.Nikare,
                                   Name = "Twin Largos"
                               },
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.Qadim,
                                   Name = "Qadim"
                               }
                           ]
                       },
                       new()
                       {
                           EncounterId = DpsReportEncounter.TheKeyOfAhdashim,
                           Name = "Key of Ahdashim",
                           Bosses =
                           [
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.Adina,
                                   Name = "Cardinal Adina"
                               },
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.Sabir,
                                   Name = "Cardinal Sabir"
                               },
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.QadimThePeerless,
                                   Name = "Qadim the Peerless"
                               }
                           ]
                       }
                   ]
               };
    }

    /// <summary>
    /// Creates the Icebrood Saga expansion entry with encounters
    /// </summary>
    /// <returns>A configured DpsReportExpansionEntry for Icebrood Saga</returns>
    private static DpsReportExpansionEntry CreateIcebroodSaga()
    {
        return new()
               {
                   ExpansionId = DpsReportExpansion.IcebroodSaga,
                   Name = "Icebrood Saga",
                   Encounters =
                   [
                       new()
                       {
                           EncounterId = DpsReportEncounter.ShiverpeaksPass,
                           Name = "Shiverpeaks Pass",
                           Bosses =
                           [
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.IcebroodConstruct,
                                   Name = "Icebrood Construct"
                               }
                           ]
                       },
                       new()
                       {
                           EncounterId = DpsReportEncounter.SanctuumArena,
                           Name = "Sanctum Arena",
                           Bosses =
                           [
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.VoiceOfTheFallen,
                                   Name = "Voice of the Fallen"
                               },
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.FraenirOfJormag,
                                   Name = "Fraenir of Jormag"
                               },
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.Boneskinner,
                                   Name = "Boneskinner"
                               }
                           ]
                       },
                       new()
                       {
                           EncounterId = DpsReportEncounter.WhisperingDepths,
                           Name = "Whispering Depths",
                           Bosses =
                           [
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.WhisperOfJormag,
                                   Name = "Whisper of Jormag"
                               }
                           ]
                       },
                       new()
                       {
                           EncounterId = DpsReportEncounter.EyeOfTheNorth,
                           Name = "Eye of the North",
                           Bosses =
                           [
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.WhisperingShadow,
                                   Name = "Forging Steel"
                               }
                           ]
                       },
                       new()
                       {
                           EncounterId = DpsReportEncounter.DrizzlewoodCoast,
                           Name = "Drizzlewood Coast",
                           Bosses =
                           [
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.WhisperingShadow,
                                   Name = "Cold War"
                               }
                           ]
                       }
                   ]
               };
    }

    /// <summary>
    /// Creates the End of Dragons expansion entry with encounters
    /// </summary>
    /// <returns>A configured DpsReportExpansionEntry for End of Dragons</returns>
    private static DpsReportExpansionEntry CreateEndOfDragons()
    {
        return new()
               {
                   ExpansionId = DpsReportExpansion.EndOfDragons,
                   Name = "End of Dragons",
                   Encounters =
                   [
                       new()
                       {
                           EncounterId = DpsReportEncounter.AetherbladHideout,
                           Name = "Aetherblade Hideout",
                           Bosses =
                           [
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.MaiTrinRaid,
                                   Name = "Mai Trin"
                               }
                           ]
                       },
                       new()
                       {
                           EncounterId = DpsReportEncounter.XunlaiJadeJunkyard,
                           Name = "Xunlai Jade Junkyard",
                           Bosses =
                           [
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.Ankka,
                                   Name = "Ankka"
                               }
                           ]
                       },
                       new()
                       {
                           EncounterId = DpsReportEncounter.KainengOverlook,
                           Name = "Kaineng Overlook",
                           Bosses =
                           [
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.MinisterLi,
                                   Name = "Minister Li"
                               }
                           ]
                       },
                       new()
                       {
                           EncounterId = DpsReportEncounter.HarvestTemple,
                           Name = "Harvest Temple",
                           Bosses =
                           [
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.VoidAmalgamate,
                                   Name = "Void Amalgamate"
                               }
                           ]
                       }
                   ]
               };
    }

    /// <summary>
    /// Creates the Secrets of the Obscure expansion entry with encounters
    /// </summary>
    /// <returns>A configured DpsReportExpansionEntry for Secrets of the Obscure</returns>
    private static DpsReportExpansionEntry CreateSecretsOfTheObscure()
    {
        return new()
               {
                   ExpansionId = DpsReportExpansion.SecretsOfTheObscure,
                   Name = "Secrets of the Obscure",
                   Encounters =
                   [
                       new()
                       {
                           EncounterId = DpsReportEncounter.CosmicObservatory,
                           Name = "Cosmic Observatory",
                           Bosses =
                           [
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.Dagda,
                                   Name = "Dagda"
                               }
                           ]
                       },
                       new()
                       {
                           EncounterId = DpsReportEncounter.TempleOfFebe,
                           Name = "Temple of Febe",
                           Bosses =
                           [
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.Cerus,
                                   Name = "Cerus"
                               }
                           ]
                       }
                   ]
               };
    }

    /// <summary>
    /// Creates the Janthir Wilds expansion entry with encounters
    /// </summary>
    /// <returns>A configured DpsReportExpansionEntry for Janthir Wilds</returns>
    private static DpsReportExpansionEntry CreateJanthirWilds()
    {
        return new()
               {
                   ExpansionId = DpsReportExpansion.JanthirWilds,
                   Name = "Janthir Wilds",
                   Encounters =
                   [
                       new()
                       {
                           EncounterId = DpsReportEncounter.MountBalrior,
                           Name = "Mount Balrior",
                           Bosses =
                           [
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.Greer,
                                   Name = "Greer"
                               },
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.Decima,
                                   Name = "Decima"
                               },
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.Ura,
                                   Name = "Ura"
                               }
                           ]
                       }
                   ]
               };
    }

    /// <summary>
    /// Creates the Visions of Eternity expansion entry with encounters
    /// </summary>
    /// <returns>A configured DpsReportExpansionEntry for Visions of Eternity</returns>
    private static DpsReportExpansionEntry CreateVisionsOfEternity()
    {
        return new()
               {
                   ExpansionId = DpsReportExpansion.VisionsOfEternity,
                   Name = "Visions of Eternity",
                   Encounters =
                   [
                       new()
                       {
                           EncounterId = DpsReportEncounter.GuardiansGlade,
                           Name = "Guardian's Glade",
                           Bosses =
                           [
                               new()
                               {
                                   BossId = SpeciesIDs.TargetID.WhisperingShadow,
                                   Name = "Kela"
                               }
                           ]
                       }
                   ]
               };
    }
}