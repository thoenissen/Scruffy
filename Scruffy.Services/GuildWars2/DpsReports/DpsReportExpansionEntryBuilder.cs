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
                                                   .WithIconUrl("https://cdn.discordapp.com/attachments/982003562462724186/1473028062408085609/GW2Logo_new.png?ex=6994b7c9&is=69936649&hm=5ee749732ecb2d332cccf364f292d69d0504d89731e0fe67a272bfe13a5f2c72&")
                                                   .WithEncounters([
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.OldLionsCourt)
                                                                                                           .WithName("Old Lion's Court")
                                                                                                           .WithIconUrl("https://assets.gw2dat.com/1451172.png")
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
                                                   .WithIconUrl("https://media.discordapp.net/attachments/982003562462724186/1473028063431229571/HoT_Texture_Centered_Trans.png?ex=6994b7c9&is=69936649&hm=c1837e5c8d6cc9823c0856dc96e44cf87edcc3196b837eaf3103716a194a588c")
                                                   .WithEncounters([
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.SpiritVale)
                                                                                                           .WithName("Spirit Vale")
                                                                                                           .WithIconUrl("https://i.imgur.com/DcuOUHQ.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.ValeGuardian).WithName("Vale Guardian").WithIconUrl("https://assets.gw2dat.com/1301792.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Gorseval).WithName("Gorseval").WithIconUrl("https://assets.gw2dat.com/1301787.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Sabetha).WithName("Sabetha").WithIconUrl("https://assets.gw2dat.com/1301795.png").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.SalvationPass)
                                                                                                           .WithName("Salvation Pass")
                                                                                                           .WithIconUrl("https://i.imgur.com/ihpaEpv.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Slothasor).WithName("Slothasor").WithIconUrl("https://assets.gw2dat.com/1377393.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Berg).WithName("Bandit Trio").WithIconUrl("https://i.imgur.com/UZZQUdf.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Matthias).WithName("Matthias").WithIconUrl("https://assets.gw2dat.com/1377391.png").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.StrongholdOfTheFaithful)
                                                                                                           .WithName("Stronghold of the Faithful")
                                                                                                           .WithIconUrl("https://i.imgur.com/i1sOswI.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.KeepConstruct).WithName("Keep Construct").WithIconUrl("https://assets.gw2dat.com/1451173.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Xera).WithName("Xera").WithIconUrl("https://assets.gw2dat.com/1451174.png").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.BastionOfThePenitent)
                                                                                                           .WithName("Bastion of the Penitent")
                                                                                                           .WithIconUrl("https://i.imgur.com/UA9F5cy.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Cairn).WithName("Cairn").WithIconUrl("https://assets.gw2dat.com/1633961.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.MursaatOverseer).WithName("Mursaat Overseer").WithIconUrl("https://assets.gw2dat.com/1633963.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Samarog).WithName("Samarog").WithIconUrl("https://assets.gw2dat.com/1633967.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Deimos).WithName("Deimos").WithIconUrl("https://i.imgur.com/KbTRa5F.png").Build())
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
                                                   .WithIconUrl("https://media.discordapp.net/attachments/982003562462724186/1473028064496717906/GW2-PoF_Texture_Centered_Trans.png?ex=6994b7ca&is=6993664a&hm=5bfbb623e748f10894d21a2c88d06197f0f6bed63cc19298cde8bc4d4ef6511a")
                                                   .WithEncounters([
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.HallOfChains)
                                                                                                           .WithName("Hall of Chains")
                                                                                                           .WithIconUrl("https://i.imgur.com/GjUeL1x.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.SoullessHorror).WithName("Soulless Horror").WithIconUrl("https://assets.gw2dat.com/1894936.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Dhuum).WithName("Voice in the Void (Dhuum)").WithIconUrl("https://assets.gw2dat.com/1894937.png").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.MythwrightGambit)
                                                                                                           .WithName("Mythwright Gambit")
                                                                                                           .WithIconUrl("https://i.imgur.com/1dSCf2T.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.ConjuredAmalgamate).WithName("Conjured Amalgamate").WithIconUrl("https://i.imgur.com/oMb2kNC.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Nikare).WithName("Twin Largos").WithIconUrl("https://i.imgur.com/6O5MT7v.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Qadim).WithName("Qadim").WithIconUrl("https://assets.gw2dat.com/2038618.png").Build())
                                                                                                           .Build(),
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.TheKeyOfAhdashim)
                                                                                                           .WithName("Key of Ahdashim")
                                                                                                           .WithIconUrl("https://i.imgur.com/3YGv1wH.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Adina).WithName("Cardinal Adina").WithIconUrl("https://assets.gw2dat.com/1766806.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Sabir).WithName("Cardinal Sabir").WithIconUrl("https://assets.gw2dat.com/1766790.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.QadimThePeerless).WithName("Qadim the Peerless").WithIconUrl("https://assets.gw2dat.com/2155914.png").Build())
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
                                                   .WithIconUrl("https://cdn.discordapp.com/attachments/982003562462724186/1473028064979193977/Living_World_Season_5_logo.png?ex=6994b7ca&is=6993664a&hm=7774ab972d29a6e510e37287f6c5d34e41798ee38e37909c8f4b61f49f9e060d&")
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
                                                   .WithIconUrl("https://cdn.discordapp.com/attachments/982003562462724186/1473028130615591165/EoD_Texture_Trans.png?ex=6994b7d9&is=69936659&hm=72707b653248852a02d453828ab073d58b26d6ec8946ad8ad1d9f5cbd23e07d3&")
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
                                                   .WithIconUrl("https://media.discordapp.net/attachments/982003562462724186/1473028131039350917/Secrets_of_the_Obscure_logo.png?ex=6994b7d9&is=69936659&hm=2dc8d9734dbdf4eeb6987e67805fc3663ab524303bb263edd84dcebd63d7fe4e")
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
                                                   .WithIconUrl("https://cdn.discordapp.com/attachments/982003562462724186/1473028131848978493/Janthir_Wilds_logo.png?ex=6994b7da&is=6993665a&hm=90b0a0e766dff30f8e099fce1be603366226a77552bd9255992d3b8962fdb46d&")
                                                   .WithEncounters([
                                                                       new DpsReportEncounterEntryBuilder().WithEncounterId(DpsReportEncounter.MountBalrior)
                                                                                                           .WithName("Mount Balrior")
                                                                                                           .WithIconUrl("https://i.imgur.com/kBVYPEV.png")
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Greer).WithName("Greer").WithIconUrl("https://i.imgur.com/19lr7VM.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Decima).WithName("Decima").WithIconUrl("https://i.imgur.com/EJJRx02.png").Build())
                                                                                                           .AddBoss(new DpsReportBossBuilder().WithBossId(SpeciesIDs.TargetID.Ura).WithName("Ura").WithIconUrl("https://i.imgur.com/hUdfF7h.png").Build())
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
                                                   .WithIconUrl("https://cdn.discordapp.com/attachments/982003562462724186/1473028132624928849/Visions_of_Eternity_logo.png?ex=6994b7da&is=6993665a&hm=2bd4184e756f20f03d0588ada1eef1caf6fd2e20688821e83b5f4abdac07cb62&")
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