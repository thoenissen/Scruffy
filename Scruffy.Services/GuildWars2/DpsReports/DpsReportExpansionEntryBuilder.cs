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
                                                   .WithEncounters([
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.OldLionsCourt)
                                                                                                           .WithName("Old Lion's Court")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.LGolem).WithName("Lion's Arch").Build())
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
                                                   .WithEncounters([
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.SpiritVale)
                                                                                                           .WithName("Spirit Vale")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.ValeGuardian).WithName("Vale Guardian").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Gorseval).WithName("Gorseval").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Sabetha).WithName("Sabetha").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.SalvationPass)
                                                                                                           .WithName("Salvation Pass")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Slothasor).WithName("Slothasor").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Berg).WithName("Bandit Trio").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Matthias).WithName("Matthias").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.StrongholdOfTheFaithful)
                                                                                                           .WithName("Stronghold of the Faithful")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.KeepConstruct).WithName("Keep Construct").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Xera).WithName("Xera").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.BastionOfThePenitent)
                                                                                                           .WithName("Bastion of the Penitent")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Cairn).WithName("Cairn").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.MursaatOverseer).WithName("Mursaat Overseer").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Samarog).WithName("Samarog").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Deimos).WithName("Deimos").Build())
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
                                                   .WithEncounters([
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.HallOfChains)
                                                                                                           .WithName("Hall of Chains")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.SoullessHorror).WithName("Soulless Horror").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Dhuum).WithName("Voice in the Void (Dhuum)").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.MythwrightGambit)
                                                                                                           .WithName("Mythwright Gambit")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.ConjuredAmalgamate).WithName("Conjured Amalgamate").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Nikare).WithName("Twin Largos").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Qadim).WithName("Qadim").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.TheKeyOfAhdashim)
                                                                                                           .WithName("Key of Ahdashim")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Adina).WithName("Cardinal Adina").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Sabir).WithName("Cardinal Sabir").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.QadimThePeerless).WithName("Qadim the Peerless").Build())
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
                                                   .WithEncounters([
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.ShiverpeaksPass)
                                                                                                           .WithName("Shiverpeaks Pass")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.IcebroodConstruct).WithName("Icebrood Construct").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.SanctuumArena)
                                                                                                           .WithName("Sanctum Arena")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.VoiceOfTheFallen).WithName("Voice of the Fallen").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.FraenirOfJormag).WithName("Fraenir of Jormag").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Boneskinner).WithName("Boneskinner").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.WhisperingDepths)
                                                                                                           .WithName("Whispering Depths")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.WhisperOfJormag).WithName("Whisper of Jormag").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.EyeOfTheNorth)
                                                                                                           .WithName("Eye of the North")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.WhisperingShadow).WithName("Forging Steel").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.DrizzlewoodCoast)
                                                                                                           .WithName("Drizzlewood Coast")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.WhisperingShadow).WithName("Cold War").Build())
                                                                                                           .Build()
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
                                                   .WithEncounters([
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.AetherbladHideout)
                                                                                                           .WithName("Aetherblade Hideout")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.MaiTrinRaid).WithName("Mai Trin").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.XunlaiJadeJunkyard)
                                                                                                           .WithName("Xunlai Jade Junkyard")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Ankka).WithName("Ankka").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.KainengOverlook)
                                                                                                           .WithName("Kaineng Overlook")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.MinisterLi).WithName("Minister Li").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.HarvestTemple)
                                                                                                           .WithName("Harvest Temple")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.VoidAmalgamate).WithName("Void Amalgamate").Build())
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
                                                   .WithEncounters([
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.CosmicObservatory)
                                                                                                           .WithName("Cosmic Observatory")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Dagda).WithName("Dagda").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.TempleOfFebe)
                                                                                                           .WithName("Temple of Febe")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Cerus).WithName("Cerus").Build())
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
                                                   .WithEncounters([
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.MountBalrior)
                                                                                                           .WithName("Mount Balrior")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Greer).WithName("Greer").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Decima).WithName("Decima").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Ura).WithName("Ura").Build())
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
                                                   .WithEncounters([
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.GuardiansGlade)
                                                                                                           .WithName("Guardian's Glade")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.WhisperingShadow).WithName("Kela").Build())
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
                   Encounters = _encounters
               };
    }

    #endregion // Methods
}