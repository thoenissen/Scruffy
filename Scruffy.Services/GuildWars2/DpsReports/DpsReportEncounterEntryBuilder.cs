using Scruffy.Data.Enumerations.DpsReport;

namespace Scruffy.Services.GuildWars2.DpsReports;

/// <summary>
/// Builder for creating DpsReportEncounterEntry instances
/// </summary>
public class DpsReportEncounterEntryBuilder
{
    #region Fields

    /// <summary>
    /// Encounter ID
    /// </summary>
    private DpsReportEncounter _encounterId;

    /// <summary>
    /// Name
    /// </summary>
    private string _name;

    /// <summary>
    /// Icon URL
    /// </summary>
    private string _iconUrl;

    /// <summary>
    /// Bosses
    /// </summary>
    private List<DpsReportBoss> _bosses = [];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Sets the encounter ID
    /// </summary>
    /// <param name="encounterId">The encounter ID</param>
    /// <returns>The current builder instance</returns>
    public DpsReportEncounterEntryBuilder WithEncounterId(DpsReportEncounter encounterId)
    {
        _encounterId = encounterId;

        return this;
    }

    /// <summary>
    /// Sets the encounter name
    /// </summary>
    /// <param name="name">The encounter name</param>
    /// <returns>The current builder instance</returns>
    public DpsReportEncounterEntryBuilder WithName(string name)
    {
        _name = name;

        return this;
    }

    /// <summary>
    /// Sets the encounter icon URL
    /// </summary>
    /// <param name="iconUrl">The encounter icon URL</param>
    /// <returns>The current builder instance</returns>
    public DpsReportEncounterEntryBuilder WithIconUrl(string iconUrl)
    {
        _iconUrl = iconUrl;

        return this;
    }

    /// <summary>
    /// Adds a boss to the encounter
    /// </summary>
    /// <param name="boss">The boss to add</param>
    /// <returns>The current builder instance</returns>
    public DpsReportEncounterEntryBuilder AddBoss(DpsReportBoss boss)
    {
        _bosses.Add(boss);

        return this;
    }

    /// <summary>
    /// Adds multiple bosses to the encounter
    /// </summary>
    /// <param name="bosses">The bosses to add</param>
    /// <returns>The current builder instance</returns>
    public DpsReportEncounterEntryBuilder AddBosses(params DpsReportBoss[] bosses)
    {
        _bosses.AddRange(bosses);

        return this;
    }

    /// <summary>
    /// Sets all bosses for the encounter
    /// </summary>
    /// <param name="bosses">The list of bosses</param>
    /// <returns>The current builder instance</returns>
    public DpsReportEncounterEntryBuilder WithBosses(List<DpsReportBoss> bosses)
    {
        _bosses = bosses ?? [];

        return this;
    }

    /// <summary>
    /// Builds the DpsReportEncounterEntry instance
    /// </summary>
    /// <returns>A new DpsReportEncounterEntry instance with the configured values</returns>
    public DpsReportEncounterEntry Build()
    {
        return new DpsReportEncounterEntry
               {
                   EncounterId = _encounterId,
                   Name = _name,
                   IconUrl = _iconUrl,
                   Bosses = _bosses
               };
    }

    #endregion // Methods
}