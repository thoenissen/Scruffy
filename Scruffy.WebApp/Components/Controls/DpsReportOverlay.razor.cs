using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using Scruffy.WebApp.Components.Pages.DpsReports.Data;

namespace Scruffy.WebApp.Components.Controls;

/// <summary>
/// Detailed statistics about a DPS Report
/// </summary>
public partial class DpsReportOverlay
{
    #region Properties

    /// <summary>
    /// DPS Report to display
    /// </summary>
    [Parameter]
    public DpsReport Report { get; set; }

    /// <summary>
    /// Callback when closing the overlay
    /// </summary>
    [Parameter]
    public EventCallback OnCloseRequested { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Gets the skill level based on the uptime percentage
    /// </summary>
    /// <param name="uptime">Uptime</param>
    /// <returns>Skill-Level CSS class</returns>
    private string GetSkillLevelFromUptime(double? uptime)
    {
        if (uptime > 80.00D)
        {
            return "skill-level-2";
        }

        if (uptime > 50.00D)
        {
            return "skill-level-1";
        }

        return "skill-level-0";
    }

    /// <summary>
    /// Gets the skill level for a mechanic based on the count
    /// </summary>
    /// <param name="count">Count</param>
    /// <returns>Skill-Level CSS class</returns>
    private string GetSkillLevelForMechanic(int count)
    {
        if (count <= 1)
        {
            return "skill-level-2";
        }

        if (count <= 3)
        {
            return "skill-level-1";
        }

        return "skill-level-0";
    }

    /// <summary>
    /// Gets the skill level from player DPS
    /// </summary>
    /// <param name="dps">DPS</param>
    /// <returns>Skill-Level CSS class</returns>
    private string GetSkillLevelFromDps(int? dps)
    {
        if (dps < 10_000)
        {
            return "skill-level-0";
        }

        if (dps < 20_000)
        {
            return "skill-level-1";
        }

        return "skill-level-2";
    }

    /// <summary>
    /// Called when the overlay close button is clicked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OnCloseOverlay()
    {
        await OnCloseRequested.InvokeAsync()
                              .ConfigureAwait(false);
    }

    #endregion // Methods
}