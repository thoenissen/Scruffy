using System.Collections.Generic;

using Scruffy.Services.GuildWars2;
using Scruffy.Services.GuildWars2.DpsReports;

namespace Scruffy.WebApp.Components.Pages.DpsReports;

/// <summary>
/// Weekly logs overview page
/// </summary>
public partial class WeeklyLogsOverviewPage
{
    #region Fields

    /// <summary>
    /// List of encounters organized by expansion
    /// </summary>
    private List<DpsReportExpansionEntry> _encounters;

    #endregion // Fields

    #region ComponentBase

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        _encounters = DpsReportAnalyzer.GetEncounters();
    }

    #endregion // ComponentBase
}