using GW2EIEvtcParser;

using Scruffy.Data.Enumerations.DpsReport;

namespace Scruffy.Services.GuildWars2.DpsReports;

/// <summary>
/// Builder for creating DpsReportExpansionEntry instances
/// </summary>
public class DpsReportExpansionEntryBuilder
{
    #region Fields

    /// <summary>
    /// Expansion ID
    /// </summary>
    private DpsReportExpansion _expansionId;

    /// <summary>
    /// Name
    /// </summary>
    private string _name;

    /// <summary>
    /// Icon URL
    /// </summary>
    private string _iconUrl;

    /// <summary>
    /// Encounters
    /// </summary>
    private List<DpsReportEncounterEntry> _encounters = [];

    #endregion // Fields

    #region Static Factory Methods

    /// <summary>
    /// Creates the Core Game expansion entry with encounters
    /// </summary>
    /// <returns>A configured DpsReportExpansionEntry for Core Game</returns>
    public static DpsReportExpansionEntry CreateCoreGame()
    {
        return new DpsReportExpansionEntryBuilder().WithExpansionId(DpsReportExpansion.CoreGame)
                                                   .WithName("Core Game")
                                                   .WithIconUrl("assets/icons/Mastery_point_(Central_Tyria).png")
                                                   .WithEncounters([
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.OldLionsCourt)
                                                                                                           .WithName("Old Lion's Court")
                                                                                                           .WithIconUrl("https://assets.gw2dat.com/1451172.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.PrototypeVermilion, SpeciesIDs.TargetID.PrototypeVermilionCM).WithName("Prototype Vermilion").Build())
                                                                                                           .Build()
                                                                   ])
                                                   .Build();
    }

    /// <summary>
    /// Creates the Heart of Thorns expansion entry with encounters
    /// </summary>
    /// <returns>A configured DpsReportExpansionEntry for Heart of Thorns</returns>
    public static DpsReportExpansionEntry CreateHeartsOfThorns()
    {
        return new DpsReportExpansionEntryBuilder().WithExpansionId(DpsReportExpansion.HeartsOfThorns)
                                                   .WithName("Heart of Thorns")
                                                   .WithIconUrl("assets/icons/Mastery_point_(Heart_of_Thorns).png")
                                                   .WithEncounters([
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.SpiritVale)
                                                                                                           .WithName("Spirit Vale")
                                                                                                           .WithIconUrl("https://i.imgur.com/DcuOUHQ.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.ValeGuardian).WithName("Vale Guardian").WithIconUrl("https://assets.gw2dat.com/1301792.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.WallOfGhosts, SpeciesIDs.TargetID.EtherealBarrierGadget).WithName("Spirit Race").WithIconUrl("https://assets.gw2dat.com/1302753.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Gorseval).WithName("Gorseval").WithIconUrl("https://assets.gw2dat.com/1301787.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Sabetha).WithName("Sabetha").WithIconUrl("https://assets.gw2dat.com/1301795.png").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.SalvationPass)
                                                                                                           .WithName("Salvation Pass")
                                                                                                           .WithIconUrl("https://i.imgur.com/ihpaEpv.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Slothasor).WithName("Slothasor").WithIconUrl("https://assets.gw2dat.com/1377393.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Berg).WithName("Bandit Trio").WithIconUrl("https://i.imgur.com/UZZQUdf.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Matthias).WithName("Matthias").WithIconUrl("https://assets.gw2dat.com/1377391.png").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.StrongholdOfTheFaithful)
                                                                                                           .WithName("Stronghold of the Faithful")
                                                                                                           .WithIconUrl("https://i.imgur.com/i1sOswI.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.McLeodTheSilent).WithName("McLeod the Silent").WithIconUrl("https://assets.gw2dat.com/1451172.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.KeepConstruct).WithName("Keep Construct").WithIconUrl("https://assets.gw2dat.com/1451173.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.HauntingStatue).WithName("Twisted Castle").WithIconUrl("https://i.imgur.com/BFFxArc.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Xera).WithName("Xera").WithIconUrl("https://assets.gw2dat.com/1451174.png").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.BastionOfThePenitent)
                                                                                                           .WithName("Bastion of the Penitent")
                                                                                                           .WithIconUrl("https://i.imgur.com/UA9F5cy.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Cairn).WithName("Cairn").WithIconUrl("https://assets.gw2dat.com/1633961.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.MursaatOverseer).WithName("Mursaat Overseer").WithIconUrl("https://assets.gw2dat.com/1633963.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Samarog).WithName("Samarog").WithIconUrl("https://assets.gw2dat.com/1633967.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Deimos).WithName("Deimos").WithIconUrl("https://i.imgur.com/KbTRa5F.png").Build())
                                                                                                           .Build()
                                                                   ])
                                                   .Build();
    }

    /// <summary>
    /// Creates the Path of Fire expansion entry with encounters
    /// </summary>
    /// <returns>A configured DpsReportExpansionEntry for Path of Fire</returns>
    public static DpsReportExpansionEntry CreatePathOfFire()
    {
        return new DpsReportExpansionEntryBuilder().WithExpansionId(DpsReportExpansion.PathOfFire)
                                                   .WithName("Path of Fire")
                                                   .WithIconUrl("assets/icons/Mastery_point_(Path_of_Fire).png")
                                                   .WithEncounters([
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.HallOfChains)
                                                                                                           .WithName("Hall of Chains")
                                                                                                           .WithIconUrl("https://i.imgur.com/GjUeL1x.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.SoullessHorror).WithName("Soulless Horror").WithIconUrl("https://assets.gw2dat.com/1894936.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Desmina).WithName("River of Souls").WithIconUrl("https://i.imgur.com/Am73toQ.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.EyeOfFate, SpeciesIDs.TargetID.EyeOfJudgement).WithName("Eye of Fate / Judgement").WithIconUrl("https://i.imgur.com/SMW6jZo.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.BrokenKing).WithName("Broken King").WithIconUrl("https://assets.gw2dat.com/1894935.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.EaterOfSouls).WithName("Eater of Souls").WithIconUrl("https://i.imgur.com/3QJU2GV.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Dhuum).WithName("Voice in the Void (Dhuum)").WithIconUrl("https://assets.gw2dat.com/1894937.png").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.MythwrightGambit)
                                                                                                           .WithName("Mythwright Gambit")
                                                                                                           .WithIconUrl("https://i.imgur.com/1dSCf2T.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.ConjuredAmalgamate).WithName("Conjured Amalgamate").WithIconUrl("https://i.imgur.com/oMb2kNC.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Nikare).WithName("Twin Largos").WithIconUrl("https://i.imgur.com/6O5MT7v.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Qadim).WithName("Qadim").WithIconUrl("https://assets.gw2dat.com/2038618.png").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.TheKeyOfAhdashim)
                                                                                                           .WithName("Key of Ahdashim")
                                                                                                           .WithIconUrl("https://i.imgur.com/3YGv1wH.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Adina).WithName("Cardinal Adina").WithIconUrl("https://assets.gw2dat.com/1766806.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Sabir).WithName("Cardinal Sabir").WithIconUrl("https://assets.gw2dat.com/1766790.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.QadimThePeerless).WithName("Qadim the Peerless").WithIconUrl("https://assets.gw2dat.com/2155914.png").Build())
                                                                                                           .Build()
                                                                   ])
                                                   .Build();
    }

    /// <summary>
    /// Creates the Icebrood Saga expansion entry with encounters
    /// </summary>
    /// <returns>A configured DpsReportExpansionEntry for Icebrood Saga</returns>
    public static DpsReportExpansionEntry CreateIcebroodSaga()
    {
        return new DpsReportExpansionEntryBuilder().WithExpansionId(DpsReportExpansion.IcebroodSaga)
                                                   .WithName("Icebrood Saga")
                                                   .WithIconUrl("assets/icons/Mastery_point_(Icebrood_Saga).png")
                                                   .WithEncounters([
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.ShiverpeaksPass)
                                                                                                           .WithName("Shiverpeaks Pass")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.IcebroodConstruct).WithName("Icebrood Construct").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.SanctuumArena)
                                                                                                           .WithName("Sanctum Arena")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.VoiceOfTheFallen, SpeciesIDs.TargetID.ClawOfTheFallen).WithName("Voice of the Fallen").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.FraenirOfJormag).WithName("Fraenir of Jormag").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Boneskinner).WithName("Boneskinner").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.WhisperingDepths)
                                                                                                           .WithName("Whispering Depths")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.WhisperOfJormag).WithName("Whisper of Jormag").Build())
                                                                                                           .Build(),
                                                                   ])
                                                   .Build();
    }

    /// <summary>
    /// Creates the End of Dragons expansion entry with encounters
    /// </summary>
    /// <returns>A configured DpsReportExpansionEntry for End of Dragons</returns>
    public static DpsReportExpansionEntry CreateEndOfDragons()
    {
        return new DpsReportExpansionEntryBuilder().WithExpansionId(DpsReportExpansion.EndOfDragons)
                                                   .WithName("End of Dragons")
                                                   .WithIconUrl("assets/icons/Mastery_point_(End_of_Dragons).png")
                                                   .WithEncounters([
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.AetherbladHideout)
                                                                                                           .WithName("Aetherblade Hideout")
                                                                                                           .WithIconUrl("https://i.imgur.com/UZmW8Sd.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.MaiTrinRaid).WithName("Mai Trin").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.XunlaiJadeJunkyard)
                                                                                                           .WithName("Xunlai Jade Junkyard")
                                                                                                           .WithIconUrl("https://i.imgur.com/orWH6qw.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Ankka).WithName("Ankka").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.KainengOverlook)
                                                                                                           .WithName("Kaineng Overlook")
                                                                                                           .WithIconUrl("https://i.imgur.com/7OutZup.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.MinisterLi, SpeciesIDs.TargetID.MinisterLiCM).WithName("Minister Li").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.HarvestTemple)
                                                                                                           .WithName("Harvest Temple")
                                                                                                           .WithIconUrl("https://i.imgur.com/gZRqzlr.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.VoidAmalgamate, SpeciesIDs.TargetID.GadgetTheDragonVoid1, SpeciesIDs.TargetID.GadgetTheDragonVoid2).WithName("Void Amalgamate").Build())
                                                                                                           .Build()
                                                                   ])
                                                   .Build();
    }

    /// <summary>
    /// Creates the Secrets of the Obscure expansion entry with encounters
    /// </summary>
    /// <returns>A configured DpsReportExpansionEntry for Secrets of the Obscure</returns>
    public static DpsReportExpansionEntry CreateSecretsOfTheObscure()
    {
        return new DpsReportExpansionEntryBuilder().WithExpansionId(DpsReportExpansion.SecretsOfTheObscure)
                                                   .WithName("Secrets of the Obscure")
                                                   .WithIconUrl("assets/icons/Mastery_point_(Secrets_of_the_Obscure).png")
                                                   .WithEncounters([
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.CosmicObservatory)
                                                                                                           .WithName("Cosmic Observatory")
                                                                                                           .WithIconUrl("https://i.imgur.com/v6vjRsH.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Dagda).WithName("Dagda").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.TempleOfFebe)
                                                                                                           .WithName("Temple of Febe")
                                                                                                           .WithIconUrl("https://i.imgur.com/1cGN1UT.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Cerus).WithName("Cerus").Build())
                                                                                                           .Build()
                                                                   ])
                                                   .Build();
    }

    /// <summary>
    /// Creates the Janthir Wilds expansion entry with encounters
    /// </summary>
    /// <returns>A configured DpsReportExpansionEntry for Janthir Wilds</returns>
    public static DpsReportExpansionEntry CreateJanthirWilds()
    {
        return new DpsReportExpansionEntryBuilder().WithExpansionId(DpsReportExpansion.JanthirWilds)
                                                   .WithName("Janthir Wilds")
                                                   .WithIconUrl("assets/icons/Mastery_point_(Janthir_Wilds).png")
                                                   .WithEncounters([
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.MountBalrior)
                                                                                                           .WithName("Mount Balrior")
                                                                                                           .WithIconUrl("https://i.imgur.com/kBVYPEV.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Greer).WithName("Greer").WithIconUrl("https://i.imgur.com/19lr7VM.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Decima, SpeciesIDs.TargetID.DecimaCM).WithName("Decima").WithIconUrl("https://i.imgur.com/EJJRx02.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.Ura).WithName("Ura").WithIconUrl("https://i.imgur.com/hUdfF7h.png").Build())
                                                                                                           .Build()
                                                                   ])
                                                   .Build();
    }

    /// <summary>
    /// Creates the Visions of Eternity expansion entry with encounters
    /// </summary>
    /// <returns>A configured DpsReportExpansionEntry for Visions of Eternity</returns>
    public static DpsReportExpansionEntry CreateVisionsOfEternity()
    {
        return new DpsReportExpansionEntryBuilder().WithExpansionId(DpsReportExpansion.VisionsOfEternity)
                                                   .WithName("Visions of Eternity")
                                                   .WithIconUrl("assets/icons/Mastery_point_(Visions_of_Eternity).png")
                                                   .WithEncounters([
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.GuardiansGlade)
                                                                                                           .WithName("Guardian's Glade")
                                                                                                           .WithIconUrl("https://i.imgur.com/7O8666G.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossIds(SpeciesIDs.TargetID.KelaSeneschalOfWaves).WithName("Kela").Build())
                                                                                                           .Build()
                                                                   ])
                                                   .Build();
    }

    #endregion // Static Factory Methods

    #region Methods

    /// <summary>
    /// Sets the expansion ID
    /// </summary>
    /// <param name="expansionId">The expansion ID</param>
    /// <returns>The current builder instance</returns>
    private DpsReportExpansionEntryBuilder WithExpansionId(DpsReportExpansion expansionId)
    {
        _expansionId = expansionId;

        return this;
    }

    /// <summary>
    /// Sets the expansion name
    /// </summary>
    /// <param name="name">The expansion name</param>
    /// <returns>The current builder instance</returns>
    private DpsReportExpansionEntryBuilder WithName(string name)
    {
        _name = name;

        return this;
    }

    /// <summary>
    /// Sets the expansion icon URL
    /// </summary>
    /// <param name="iconUrl">The encounter icon URL</param>
    /// <returns>The current builder instance</returns>
    private DpsReportExpansionEntryBuilder WithIconUrl(string iconUrl)
    {
        _iconUrl = iconUrl;

        return this;
    }

    /// <summary>
    /// Sets all encounters for the expansion
    /// </summary>
    /// <param name="encounters">The list of encounters</param>
    /// <returns>The current builder instance</returns>
    private DpsReportExpansionEntryBuilder WithEncounters(List<DpsReportEncounterEntry> encounters)
    {
        _encounters = encounters ?? [];

        return this;
    }

    /// <summary>
    /// Builds the DpsReportExpansionEntry instance
    /// </summary>
    /// <returns>A new DpsReportExpansionEntry instance with the configured values</returns>
    private DpsReportExpansionEntry Build()
    {
        return new DpsReportExpansionEntry
               {
                   ExpansionId = _expansionId,
                   Name = _name,
                   IconUrl = _iconUrl,
                   Encounters = _encounters
               };
    }

    #endregion // Methods
}