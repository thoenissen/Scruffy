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
    /// Loading hint
    /// </summary>
    [Parameter]
    public string Hint { get; set; }

    /// <summary>
    /// Content
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; }

    /// <summary>
    /// Size of the loading animation
    /// </summary>
    [Parameter]
    public LoadingIndicatorSize Size { get; set; } = LoadingIndicatorSize.Medium;

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Get the CSS classes for the spinner
    /// </summary>
    /// <returns>CSS classes</returns>
    private string GetSpinnerClasses()
    {
        return Size == LoadingIndicatorSize.Small
                   ? "spinner spinner-small"
                   : "spinner spinner-medium";
    }

    #endregion // Methods

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