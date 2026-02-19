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
    private SpeciesIDs.TargetID[] _bossIds;

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
    /// Sets the boss IDs
    /// </summary>
    /// <param name="bossIds">The boss IDs</param>
    /// <returns>The current builder instance</returns>
    public DpsReportBossBuilder WithBossIds(params SpeciesIDs.TargetID[] bossIds)
    {
        _bossIds = bossIds;

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
                   BossIds = _bossIds,
                   Name = _name,
                   IconUrl = _iconUrl
               };
    }

    #endregion // Methods
}