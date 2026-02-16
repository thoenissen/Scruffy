using GW2EIEvtcParser;

namespace Scruffy.Services.GuildWars2.DpsReports;

/// <summary>
/// Builder for creating DpsReportBoss instances
/// </summary>
public class DpsReportBossBuilder
{
    #region Fields

    /// <summary>
    /// Boss ID
    /// </summary>
    private SpeciesIDs.TargetID _bossId;

    /// <summary>
    /// Name
    /// </summary>
    private string _name;

    /// <summary>
    /// Icon URL
    /// </summary>
    private string _iconUrl;

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Sets the boss ID
    /// </summary>
    /// <param name="bossId">The boss ID</param>
    /// <returns>The current builder instance</returns>
    public DpsReportBossBuilder WithBossId(SpeciesIDs.TargetID bossId)
    {
        _bossId = bossId;

        return this;
    }

    /// <summary>
    /// Sets the boss name
    /// </summary>
    /// <param name="name">The boss name</param>
    /// <returns>The current builder instance</returns>
    public DpsReportBossBuilder WithName(string name)
    {
        _name = name;

        return this;
    }

    /// <summary>
    /// Sets the boss icon URL
    /// </summary>
    /// <param name="iconUrl">The boss icon URL</param>
    /// <returns>The current builder instance</returns>
    public DpsReportBossBuilder WithIconUrl(string iconUrl)
    {
        _iconUrl = iconUrl;

        return this;
    }

    /// <summary>
    /// Builds the DpsReportBoss instance
    /// </summary>
    /// <returns>A new DpsReportBoss instance with the configured values</returns>
    public DpsReportBoss Build()
    {
        return new DpsReportBoss
               {
                   BossId = _bossId,
                   Name = _name,
                   IconUrl = _iconUrl
               };
    }

    #endregion // Methods
}