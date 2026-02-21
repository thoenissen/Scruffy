using System;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using Scruffy.Data.Converter;
using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.Services.WebApi;
using Scruffy.WebApp.DTOs.Administration;

namespace Scruffy.WebApp.Components.Controls;

/// <summary>
/// Overlay for editing user configuration
/// </summary>
public sealed partial class UserConfigurationOverlay : IDisposable
{
    #region Fields

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _repositoryFactory = RepositoryFactory.CreateInstance();

    /// <summary>
    /// Is a save operation in progress?
    /// </summary>
    private bool _isSaving;

    /// <summary>
    /// Is an API key validation in progress?
    /// </summary>
    private bool _isValidating;

    /// <summary>
    /// Validation result message
    /// </summary>
    private string _validationMessage;

    /// <summary>
    /// Was the validation successful?
    /// </summary>
    private bool _validationSuccess;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// User to configure
    /// </summary>
    [Parameter]
    public UserDTO User { get; set; }

    /// <summary>
    /// Callback when closing the overlay
    /// </summary>
    [Parameter]
    public EventCallback OnCloseRequested { get; set; }

    /// <summary>
    /// Callback when the configuration has been changed
    /// </summary>
    [Parameter]
    public EventCallback OnConfigurationChanged { get; set; }

    /// <summary>
    /// HTTP-Client factory
    /// </summary>
    [Inject]
    public IHttpClientFactory HttpClientFactory { get; set; }

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

    /// <summary>
    /// Toggles the fixed rank setting
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OnToggleFixedRank()
    {
        _isSaving = true;

        try
        {
            User.IsFixedRank = User.IsFixedRank == false;

            SaveConfiguration();

            await OnConfigurationChanged.InvokeAsync()
                                        .ConfigureAwait(false);
        }
        finally
        {
            _isSaving = false;
            await InvokeAsync(StateHasChanged)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Toggles the inactive setting
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OnToggleInactive()
    {
        _isSaving = true;

        try
        {
            User.IsInactive = User.IsInactive == false;

            SaveConfiguration();

            await OnConfigurationChanged.InvokeAsync()
                                        .ConfigureAwait(false);
        }
        finally
        {
            _isSaving = false;
            await InvokeAsync(StateHasChanged)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Saves the current configuration to the database
    /// </summary>
    private void SaveConfiguration()
    {
        if (User.UserId.HasValue
            && User.GuildId.HasValue)
        {
            _repositoryFactory.GetRepository<GuildUserConfigurationRepository>()
                              .AddOrRefresh(obj => obj.UserId == User.UserId.Value
                                                   && obj.GuildId == User.GuildId.Value,
                                            obj =>
                                            {
                                                obj.GuildId = User.GuildId.Value;
                                                obj.UserId = User.UserId.Value;
                                                obj.IsFixedRank = User.IsFixedRank;
                                                obj.IsInactive = User.IsInactive;
                                            });
        }
    }

    /// <summary>
    /// Validates the API key against the Guild Wars 2 API
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OnValidateApiKey()
    {
        _isValidating = true;
        _validationMessage = null;

        try
        {
            var connector = new GuildWars2ApiConnector(HttpClientFactory.CreateClient(), User.GuildWarsAccountApiKey);

            await using (connector.ConfigureAwait(false))
            {
                var tokenInformation = await connector.GetTokenInformationAsync()
                                                      .ConfigureAwait(false);

                var permissions = GuildWars2ApiDataConverter.ToPermission(tokenInformation?.Permissions);

                _repositoryFactory.GetRepository<GuildWarsAccountRepository>()
                                  .Refresh(obj => obj.ApiKey == User.GuildWarsAccountApiKey,
                                           obj => obj.Permissions = permissions);

                var isValid = permissions.HasFlag(GuildWars2ApiPermission.RequiredPermissions);

                User.IsApiKeyValid = isValid;
                _validationSuccess = isValid;
                _validationMessage = isValid ? LocalizationGroup.GetText("ValidationSuccess", "API key is valid.") : LocalizationGroup.GetText("ValidationInvalidPermissions", "API key has insufficient permissions.");
            }
        }
        catch (Exception)
        {
            _validationSuccess = false;
            _validationMessage = LocalizationGroup.GetText("ValidationFailed", "API key validation failed.");

            User.IsApiKeyValid = false;
        }
        finally
        {
            _isValidating = false;
        }
    }

    #endregion // Methods

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose()
    {
        _repositoryFactory?.Dispose();
    }

    #endregion // IDisposable
}