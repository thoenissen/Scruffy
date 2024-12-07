using System;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Scruffy.WebApp.Components.Layout;

/// <summary>
/// Top menu
/// </summary>
public sealed partial class TopMenu : IDisposable
{
    #region Fields

    /// <summary>
    /// Current url
    /// </summary>
    private string _currentUrl;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Navigation manager
    /// </summary>
    [Inject]
    private NavigationManager NavigationManager { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Reaction to <see cref="NavigationManager.LocationChanged"/>
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Arguments</param>
    private void OnLocationChanged(object sender, LocationChangedEventArgs e)
    {
        _currentUrl = NavigationManager.ToBaseRelativePath(e.Location);

        StateHasChanged();
    }

    #endregion // Methods

    #region ComponentBase

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        _currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);

        NavigationManager.LocationChanged += OnLocationChanged;
    }

    #endregion // ComponentBase

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }

    #endregion // IDisposable
}