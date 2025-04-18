using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Scruffy.Data.Json.ChartJs;

namespace Scruffy.WebApp.Components.Controls;

/// <summary>
/// Chart component
/// </summary>
public partial class Chart : IAsyncDisposable
{
    #region Fields

    /// <summary>
    /// Unique ID for the chart
    /// </summary>
    private readonly string _id = $"chart-{Guid.NewGuid()}";

    /// <summary>
    /// JavaScript module reference for the chart
    /// </summary>
    private IJSObjectReference _module;

    /// <summary>
    /// JavaScript object reference for the chart instance
    /// </summary>
    private IJSObjectReference _instance;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Type of the chart
    /// </summary>
    [Parameter]
    public string Type { get; set; }

    /// <summary>
    /// Chart data
    /// </summary>
    [Parameter]
    public ChartData Data { get; set; }

    /// <summary>
    /// Chart options
    /// </summary>
    [Parameter]
    public ChartOptions Options { get; set; }

    /// <summary>
    /// Title of the chart
    /// </summary>
    [Parameter]
    public string Title { get; set; }

    /// <summary>
    /// Description of the chart
    /// </summary>
    [Parameter]
    public string Description { get; set; }

    /// <summary>
    /// JavaScript runtime
    /// </summary>
    [Inject]
    public IJSRuntime Runtime { get; set; }

    #endregion // Properties

    #region ComponentBase

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await Runtime.InvokeAsync<IJSObjectReference>("import", "./js/chartInterop.js")
                                   .ConfigureAwait(false);

            if (_module != null)
            {
                _instance = await _module.InvokeAsync<IJSObjectReference>("createChart", _id, Type, Data, Options)
                                         .ConfigureAwait(false);
            }
        }
    }

    #endregion // ComponentBase

    #region IAsyncDisposable

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_instance != null)
        {
            await _instance.InvokeVoidAsync("destroy")
                           .ConfigureAwait(false);
            await _instance.DisposeAsync()
                           .ConfigureAwait(false);
        }

        if (_module != null)
        {
            await _module.DisposeAsync()
                         .ConfigureAwait(false);
        }
    }

    #endregion // IAsyncDisposable
}