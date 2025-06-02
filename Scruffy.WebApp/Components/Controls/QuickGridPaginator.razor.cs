using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;

namespace Scruffy.WebApp.Components.Controls;

/// <summary>
/// Paginator
/// </summary>
public partial class QuickGridPaginator
{
    #region Fields

    /// <summary>
    /// The last state object (<see cref="State"/>)
    /// </summary>
    private PaginationState _lastState;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Specifies the associated <see cref="PaginationState"/>
    /// </summary>
    [Parameter]
    public PaginationState State { get; set; } = null!;

    /// <summary>
    /// Total item count
    /// </summary>
    private int TotalItemCount => State.TotalItemCount ?? 0;

    #endregion // Properties

    /// <summary>
    /// Go to the first page asynchronously
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private Task GoFirstAsync()
    {
        return GoToPageAsync(0);
    }

    /// <summary>
    /// Go to the previous page asynchronously
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private Task GoPreviousAsync()
    {
        return GoToPageAsync(State.CurrentPageIndex - 1);
    }

    /// <summary>
    /// Go to the next page asynchronously
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private Task GoNextAsync()
    {
        return GoToPageAsync(State.CurrentPageIndex + 1);
    }

    /// <summary>
    /// Go to the last page asynchronously
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private Task GoLastAsync()
    {
        return GoToPageAsync(State.LastPageIndex.GetValueOrDefault(0));
    }

    /// <summary>
    /// Checks if it is possible to go back in the pagination
    /// </summary>
    private bool CanGoBack => State.CurrentPageIndex > 0;

    /// <summary>
    /// Checks if it is possible to go forwards in the pagination
    /// </summary>
    private bool CanGoForwards => State.CurrentPageIndex < State.LastPageIndex;

    /// <summary>
    /// Go to a specific page asynchronously
    /// </summary>
    /// <param name="pageIndex">Page index</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private Task GoToPageAsync(int pageIndex)
    {
        return State.SetCurrentPageIndexAsync(pageIndex);
    }

    /// <summary>
    /// Event handler for when the total item count has changed
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Arguments</param>
    private void OnTotalItemCountChanged(object sender, int? e)
    {
        InvokeAsync(StateHasChanged);
    }

    #region ComponentBase

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (_lastState != null)
        {
            _lastState.TotalItemCountChanged -= OnTotalItemCountChanged;
        }

        _lastState = State;

        if (State != null)
        {
            State.TotalItemCountChanged += OnTotalItemCountChanged;
        }
    }

    /// <inheritdoc/>
    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        if (firstRender)
        {
            StateHasChanged();
        }
    }

    #endregion // ComponentBase
}