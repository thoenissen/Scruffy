using GW2EIEvtcParser;

using Scruffy.Data.Enumerations.DpsReport;

namespace Scruffy.Data.Services.DpsReport;

/// <summary>
/// Encounter grouping key
/// </summary>
public class DpsReportEncounterKey
{
    #region Properties

    /// <summary>
    /// Group
    /// </summary>
    public DpsReportGroup Group { get; set; }

    /// <summary>
    /// Subgroup
    /// </summary>
    public DpsReportSubGroup SubGroup { get; set; }

    /// <summary>
    /// Encounter
    /// </summary>
    public DpsReportEncounterTarget Encounter { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Creation of the key from a boss ID
    /// </summary>
    /// <param name="bossId">Boss ID</param>
    /// <returns>Key</returns>
    public static DpsReportEncounterKey FromBossId(long bossId)
    {
        return (SpeciesIDs.TargetID)bossId switch
               {
                   // Fractals - Nightmare
                   SpeciesIDs.TargetID.MAMA => new DpsReportEncounterKey
                                               {
                                                   Group = DpsReportGroup.Fractals,
                                                   SubGroup = DpsReportSubGroup.Nightmare,
                                                   Encounter = DpsReportEncounterTarget.MAMA
                                               },
                   SpeciesIDs.TargetID.Siax => new DpsReportEncounterKey
                                               {
                                                   Group = DpsReportGroup.Fractals,
                                                   SubGroup = DpsReportSubGroup.Nightmare,
                                                   Encounter = DpsReportEncounterTarget.Siax
                                               },
                   SpeciesIDs.TargetID.Ensolyss => new DpsReportEncounterKey
                                                   {
                                                       Group = DpsReportGroup.Fractals,
                                                       SubGroup = DpsReportSubGroup.Nightmare,
                                                       Encounter = DpsReportEncounterTarget.Ensolyss
                                                   },

                   // Fractals - Shattered Observatory
                   SpeciesIDs.TargetID.Skorvald => new DpsReportEncounterKey
                                                   {
                                                       Group = DpsReportGroup.Fractals,
                                                       SubGroup = DpsReportSubGroup.ShatteredObservatory,
                                                       Encounter = DpsReportEncounterTarget.Skorvald
                                                   },
                   SpeciesIDs.TargetID.Artsariiv => new DpsReportEncounterKey
                                                    {
                                                        Group = DpsReportGroup.Fractals,
                                                        SubGroup = DpsReportSubGroup.ShatteredObservatory,
                                                        Encounter = DpsReportEncounterTarget.Artsariiv
                                                    },
                   SpeciesIDs.TargetID.Arkk => new DpsReportEncounterKey
                                               {
                                                   Group = DpsReportGroup.Fractals,
                                                   SubGroup = DpsReportSubGroup.ShatteredObservatory,
                                                   Encounter = DpsReportEncounterTarget.Arkk
                                               },

                   // Fractals - Sunqua Peak
                   SpeciesIDs.TargetID.AiKeeperOfThePeak => new DpsReportEncounterKey
                                                            {
                                                                Group = DpsReportGroup.Fractals,
                                                                SubGroup = DpsReportSubGroup.SunquaPeak,
                                                                Encounter = DpsReportEncounterTarget.AiKeeperOfThePeak
                                                            },

                   // Fractals - Silent Surf
                   SpeciesIDs.TargetID.KanaxaiScytheOfHouseAurkusNM
                       or SpeciesIDs.TargetID.KanaxaiScytheOfHouseAurkusCM => new DpsReportEncounterKey
                                                                              {
                                                                                  Group = DpsReportGroup.Fractals,
                                                                                  SubGroup = DpsReportSubGroup.SilentSurf,
                                                                                  Encounter = DpsReportEncounterTarget.Kanaxai
                                                                              },

                   // Fractals - Lonely Tower
                   SpeciesIDs.TargetID.CerusLonelyTower => new DpsReportEncounterKey
                                                           {
                                                               Group = DpsReportGroup.Fractals,
                                                               SubGroup = DpsReportSubGroup.LonelyTower,
                                                               Encounter = DpsReportEncounterTarget.CerusLonelyTower
                                                           },
                   SpeciesIDs.TargetID.DeimosLonelyTower => new DpsReportEncounterKey
                                                            {
                                                                Group = DpsReportGroup.Fractals,
                                                                SubGroup = DpsReportSubGroup.LonelyTower,
                                                                Encounter = DpsReportEncounterTarget.DeimosLonelyTower
                                                            },
                   SpeciesIDs.TargetID.EparchLonelyTower => new DpsReportEncounterKey
                                                            {
                                                                Group = DpsReportGroup.Fractals,
                                                                SubGroup = DpsReportSubGroup.LonelyTower,
                                                                Encounter = DpsReportEncounterTarget.EparchLonelyTower
                                                            },

                   // Fractals - Kinfall
                   SpeciesIDs.TargetID.WhisperingShadow => new DpsReportEncounterKey
                                                           {
                                                               Group = DpsReportGroup.Fractals,
                                                               SubGroup = DpsReportSubGroup.Kinfall,
                                                               Encounter = DpsReportEncounterTarget.WhisperingShadow
                                                           },

                   // IBS Strikes
                   SpeciesIDs.TargetID.IcebroodConstruct
                   or SpeciesIDs.TargetID.IcebroodConstructFraenir => new DpsReportEncounterKey
                                                                      {
                                                                          Group = DpsReportGroup.IcebroodSaga,
                                                                          Encounter = DpsReportEncounterTarget.IcebroodConstruct
                                                                      },
                   SpeciesIDs.TargetID.VoiceOfTheFallen
                   or SpeciesIDs.TargetID.ClawOfTheFallen => new DpsReportEncounterKey
                                                             {
                                                                 Group = DpsReportGroup.IcebroodSaga,
                                                                 Encounter = DpsReportEncounterTarget.VoiceAndClawOfTheFallen
                                                             },
                   SpeciesIDs.TargetID.FraenirOfJormag => new DpsReportEncounterKey
                                                          {
                                                              Group = DpsReportGroup.IcebroodSaga,
                                                              Encounter = DpsReportEncounterTarget.FraenirOfJormag
                                                          },
                   SpeciesIDs.TargetID.WhisperOfJormag => new DpsReportEncounterKey
                                                          {
                                                              Group = DpsReportGroup.IcebroodSaga,
                                                              Encounter = DpsReportEncounterTarget.WhisperOfJormag
                                                          },
                   SpeciesIDs.TargetID.VariniaStormsounder => new DpsReportEncounterKey
                                                              {
                                                                  Group = DpsReportGroup.IcebroodSaga,
                                                                  Encounter = DpsReportEncounterTarget.VariniaStormsounder
                                                              },
                   SpeciesIDs.TargetID.Boneskinner => new DpsReportEncounterKey
                                                      {
                                                          Group = DpsReportGroup.IcebroodSaga,
                                                          Encounter = DpsReportEncounterTarget.Boneskinner
                                                      },

                   // EoD Strikes
                   SpeciesIDs.TargetID.MaiTrinRaid
                   or SpeciesIDs.TargetID.EchoOfScarletBriarNM
                   or SpeciesIDs.TargetID.EchoOfScarletBriarCM => new DpsReportEncounterKey
                                                                  {
                                                                      Group = DpsReportGroup.EndOfDragons,
                                                                      Encounter = DpsReportEncounterTarget.MaiTrin
                                                                  },
                   SpeciesIDs.TargetID.Ankka => new DpsReportEncounterKey
                                                {
                                                    Group = DpsReportGroup.EndOfDragons,
                                                    Encounter = DpsReportEncounterTarget.Ankka
                                                },
                   SpeciesIDs.TargetID.MinisterLi
                   or SpeciesIDs.TargetID.MinisterLiCM => new DpsReportEncounterKey
                                                          {
                                                              Group = DpsReportGroup.EndOfDragons,
                                                              Encounter = DpsReportEncounterTarget.MinisterLi
                                                          },
                   SpeciesIDs.TargetID.GadgetTheDragonVoid1
                   or SpeciesIDs.TargetID.GadgetTheDragonVoid2
                   or SpeciesIDs.TargetID.VoidAmalgamate => new DpsReportEncounterKey
                                                            {
                                                                Group = DpsReportGroup.EndOfDragons,
                                                                Encounter = DpsReportEncounterTarget.VoidAmalgamate
                                                            },
                   SpeciesIDs.TargetID.PrototypeVermilionCM => new DpsReportEncounterKey
                                                               {
                                                                   Group = DpsReportGroup.EndOfDragons,
                                                                   Encounter = DpsReportEncounterTarget.PrototypeVermilion
                                                               },

                   // SotO Strikes
                   SpeciesIDs.TargetID.Dagda => new DpsReportEncounterKey
                                                {
                                                    Group = DpsReportGroup.SecretsOfTheObscure,
                                                    Encounter = DpsReportEncounterTarget.Dagda
                                                },
                   SpeciesIDs.TargetID.Cerus => new DpsReportEncounterKey
                                                {
                                                    Group = DpsReportGroup.SecretsOfTheObscure,
                                                    Encounter = DpsReportEncounterTarget.Cerus
                                                },

                   // Training Area
                   SpeciesIDs.TargetID.MassiveGolem10M
                   or SpeciesIDs.TargetID.MassiveGolem4M
                   or SpeciesIDs.TargetID.MassiveGolem1M
                   or SpeciesIDs.TargetID.VitalGolem
                   or SpeciesIDs.TargetID.AvgGolem
                   or SpeciesIDs.TargetID.StdGolem
                   or SpeciesIDs.TargetID.LGolem
                   or SpeciesIDs.TargetID.MedGolem
                   or SpeciesIDs.TargetID.ConditionGolem
                   or SpeciesIDs.TargetID.PowerGolem => new DpsReportEncounterKey
                                                        {
                                                            Group = DpsReportGroup.TrainingArea,
                                                            Encounter = DpsReportEncounterTarget.TrainingsGolem
                                                        },

                   // Wing 1 - Spirit Vale
                   SpeciesIDs.TargetID.ValeGuardian => new DpsReportEncounterKey
                                                       {
                                                           Group = DpsReportGroup.HeartsOfThorns,
                                                           SubGroup = DpsReportSubGroup.SpiritVale,
                                                           Encounter = DpsReportEncounterTarget.ValeGuardian
                                                       },
                   SpeciesIDs.TargetID.Gorseval => new DpsReportEncounterKey
                                                   {
                                                       Group = DpsReportGroup.HeartsOfThorns,
                                                       SubGroup = DpsReportSubGroup.SpiritVale,
                                                       Encounter = DpsReportEncounterTarget.Gorseval
                                                   },
                   SpeciesIDs.TargetID.Sabetha => new DpsReportEncounterKey
                                                  {
                                                      Group = DpsReportGroup.HeartsOfThorns,
                                                      SubGroup = DpsReportSubGroup.SpiritVale,
                                                      Encounter = DpsReportEncounterTarget.Sabetha
                                                  },

                   // Wing 2 - Salvation Pass
                   SpeciesIDs.TargetID.Slothasor => new DpsReportEncounterKey
                                                    {
                                                        Group = DpsReportGroup.HeartsOfThorns,
                                                        SubGroup = DpsReportSubGroup.SalvationPass,
                                                        Encounter = DpsReportEncounterTarget.Slothasor
                                                    },
                   SpeciesIDs.TargetID.Berg
                   or SpeciesIDs.TargetID.Zane
                   or SpeciesIDs.TargetID.Narella => new DpsReportEncounterKey
                                                     {
                                                         Group = DpsReportGroup.HeartsOfThorns,
                                                         SubGroup = DpsReportSubGroup.SalvationPass,
                                                         Encounter = DpsReportEncounterTarget.BanditTrio
                                                     },
                   SpeciesIDs.TargetID.Matthias => new DpsReportEncounterKey
                                                   {
                                                       Group = DpsReportGroup.HeartsOfThorns,
                                                       SubGroup = DpsReportSubGroup.SalvationPass,
                                                       Encounter = DpsReportEncounterTarget.Matthias
                                                   },

                   // Wing 3 - Stronghold of the Faithful
                   SpeciesIDs.TargetID.McLeodTheSilent => new DpsReportEncounterKey
                                                          {
                                                              Group = DpsReportGroup.HeartsOfThorns,
                                                              SubGroup = DpsReportSubGroup.StrongholdOfTheFaithful,
                                                              Encounter = DpsReportEncounterTarget.McLeodTheSilent
                                                          },
                   SpeciesIDs.TargetID.KeepConstruct => new DpsReportEncounterKey
                                                        {
                                                            Group = DpsReportGroup.HeartsOfThorns,
                                                            SubGroup = DpsReportSubGroup.StrongholdOfTheFaithful,
                                                            Encounter = DpsReportEncounterTarget.KeepConstruct
                                                        },
                   SpeciesIDs.TargetID.HauntingStatue => new DpsReportEncounterKey
                                                         {
                                                             Group = DpsReportGroup.HeartsOfThorns,
                                                             SubGroup = DpsReportSubGroup.StrongholdOfTheFaithful,
                                                             Encounter = DpsReportEncounterTarget.HauntingStatue
                                                         },
                   SpeciesIDs.TargetID.Xera => new DpsReportEncounterKey
                                               {
                                                   Group = DpsReportGroup.HeartsOfThorns,
                                                   SubGroup = DpsReportSubGroup.StrongholdOfTheFaithful,
                                                   Encounter = DpsReportEncounterTarget.Xera
                                               },

                   // Wing 4 - Bastion of the Penitent
                   SpeciesIDs.TargetID.Cairn => new DpsReportEncounterKey
                                                {
                                                    Group = DpsReportGroup.HeartsOfThorns,
                                                    SubGroup = DpsReportSubGroup.BastionOfThePenitent,
                                                    Encounter = DpsReportEncounterTarget.Cairn
                                                },
                   SpeciesIDs.TargetID.MursaatOverseer => new DpsReportEncounterKey
                                                          {
                                                              Group = DpsReportGroup.HeartsOfThorns,
                                                              SubGroup = DpsReportSubGroup.BastionOfThePenitent,
                                                              Encounter = DpsReportEncounterTarget.MursaatOverseer
                                                          },
                   SpeciesIDs.TargetID.Samarog => new DpsReportEncounterKey
                                                  {
                                                      Group = DpsReportGroup.HeartsOfThorns,
                                                      SubGroup = DpsReportSubGroup.BastionOfThePenitent,
                                                      Encounter = DpsReportEncounterTarget.Samarog
                                                  },
                   SpeciesIDs.TargetID.Deimos => new DpsReportEncounterKey
                                                 {
                                                     Group = DpsReportGroup.HeartsOfThorns,
                                                     SubGroup = DpsReportSubGroup.BastionOfThePenitent,
                                                     Encounter = DpsReportEncounterTarget.Deimos
                                                 },

                   // Wing 5 - Hall of Chains
                   SpeciesIDs.TargetID.SoullessHorror => new DpsReportEncounterKey
                                                         {
                                                             Group = DpsReportGroup.PathOfFire,
                                                             SubGroup = DpsReportSubGroup.HallOfChains,
                                                             Encounter = DpsReportEncounterTarget.SoullessHorror
                                                         },
                   SpeciesIDs.TargetID.Desmina => new DpsReportEncounterKey
                                                  {
                                                      Group = DpsReportGroup.PathOfFire,
                                                      SubGroup = DpsReportSubGroup.HallOfChains,
                                                      Encounter = DpsReportEncounterTarget.Desmina
                                                  },
                   SpeciesIDs.TargetID.BrokenKing => new DpsReportEncounterKey
                                                     {
                                                         Group = DpsReportGroup.PathOfFire,
                                                         SubGroup = DpsReportSubGroup.HallOfChains,
                                                         Encounter = DpsReportEncounterTarget.BrokenKing
                                                     },
                   SpeciesIDs.TargetID.EaterOfSouls => new DpsReportEncounterKey
                                                       {
                                                           Group = DpsReportGroup.PathOfFire,
                                                           SubGroup = DpsReportSubGroup.HallOfChains,
                                                           Encounter = DpsReportEncounterTarget.EaterOfSouls
                                                       },
                   SpeciesIDs.TargetID.EyeOfJudgement => new DpsReportEncounterKey
                                                         {
                                                             Group = DpsReportGroup.PathOfFire,
                                                             SubGroup = DpsReportSubGroup.HallOfChains,
                                                             Encounter = DpsReportEncounterTarget.EyeOfJudgement
                                                         },
                   SpeciesIDs.TargetID.EyeOfFate => new DpsReportEncounterKey
                                                    {
                                                        Group = DpsReportGroup.PathOfFire,
                                                        SubGroup = DpsReportSubGroup.HallOfChains,
                                                        Encounter = DpsReportEncounterTarget.EyeOfFate
                                                    },
                   SpeciesIDs.TargetID.Dhuum => new DpsReportEncounterKey
                                                {
                                                    Group = DpsReportGroup.PathOfFire,
                                                    SubGroup = DpsReportSubGroup.HallOfChains,
                                                    Encounter = DpsReportEncounterTarget.Dhuum
                                                },

                   // Wing 6 - Mythwright Gambit
                   SpeciesIDs.TargetID.ConjuredAmalgamate
                   or SpeciesIDs.TargetID.CARightArm
                   or SpeciesIDs.TargetID.CALeftArm => new DpsReportEncounterKey
                                                       {
                                                           Group = DpsReportGroup.PathOfFire,
                                                           SubGroup = DpsReportSubGroup.MythwrightGambit,
                                                           Encounter = DpsReportEncounterTarget.ConjuredAmalgamate
                                                       },
                   SpeciesIDs.TargetID.Nikare
                   or SpeciesIDs.TargetID.Kenut => new DpsReportEncounterKey
                                                   {
                                                       Group = DpsReportGroup.PathOfFire,
                                                       SubGroup = DpsReportSubGroup.MythwrightGambit,
                                                       Encounter = DpsReportEncounterTarget.TwinLargos
                                                   },
                   SpeciesIDs.TargetID.Qadim => new DpsReportEncounterKey
                                                {
                                                    Group = DpsReportGroup.PathOfFire,
                                                    SubGroup = DpsReportSubGroup.MythwrightGambit,
                                                    Encounter = DpsReportEncounterTarget.Qadim
                                                },

                   // Wing 7 - The Key of Ahdashim
                   SpeciesIDs.TargetID.Adina => new DpsReportEncounterKey
                                                {
                                                    Group = DpsReportGroup.PathOfFire,
                                                    SubGroup = DpsReportSubGroup.TheKeyOfAhdashim,
                                                    Encounter = DpsReportEncounterTarget.Adina
                                                },
                   SpeciesIDs.TargetID.Sabir => new DpsReportEncounterKey
                                                {
                                                    Group = DpsReportGroup.PathOfFire,
                                                    SubGroup = DpsReportSubGroup.TheKeyOfAhdashim,
                                                    Encounter = DpsReportEncounterTarget.Sabir
                                                },
                   SpeciesIDs.TargetID.QadimThePeerless => new DpsReportEncounterKey
                                                           {
                                                               Group = DpsReportGroup.PathOfFire,
                                                               SubGroup = DpsReportSubGroup.TheKeyOfAhdashim,
                                                               Encounter = DpsReportEncounterTarget.QadimThePeerless
                                                           },

                   // Wing 8 - Mount Balrior
                   SpeciesIDs.TargetID.Decima => new DpsReportEncounterKey
                                                 {
                                                     Group = DpsReportGroup.JanthirWilds,
                                                     SubGroup = DpsReportSubGroup.MountBalrior,
                                                     Encounter = DpsReportEncounterTarget.Decima
                                                 },
                   SpeciesIDs.TargetID.Greer => new DpsReportEncounterKey
                                                {
                                                    Group = DpsReportGroup.JanthirWilds,
                                                    SubGroup = DpsReportSubGroup.MountBalrior,
                                                    Encounter = DpsReportEncounterTarget.Greer
                                                },
                   SpeciesIDs.TargetID.Ura => new DpsReportEncounterKey
                                              {
                                                  Group = DpsReportGroup.JanthirWilds,
                                                  SubGroup = DpsReportSubGroup.MountBalrior,
                                                  Encounter = DpsReportEncounterTarget.Ura
                                              },

                   _ => new DpsReportEncounterKey()
               };
    }

    #endregion // Methods
}