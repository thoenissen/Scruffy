using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using Scruffy.Data.Enumerations.DpsReport;
using Scruffy.WebApp.Components.Services.DpsReports;

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

    /// <summary>
    /// Report visualizer
    /// </summary>
    [Inject]
    private DpsReportVisualizer DpsReportVisualizer { get; set; }

    #endregion // Properties

    #region Methods

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