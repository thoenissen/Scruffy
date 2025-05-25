using Microsoft.AspNetCore.Components;

namespace Scruffy.WebApp.Components.Controls;

/// <summary>
/// Loading animation
/// </summary>
public partial class LoadingContainer
{
    #region Fields

    /// <summary>
    /// Flag indicating whether the component has been initialized
    /// </summary>
    private bool _isInitialized;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Is the content loading?
    /// </summary>
    [Parameter]
    public bool IsLoading { get; set; }

    /// <summary>
    /// Content
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; }

    #endregion // Properties

    #region ComponentBase

    /// <inheritdoc />
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender == false)
        {
            return;
        }

        _isInitialized = true;

        StateHasChanged();
    }

    #endregion // ComponentBase
}