using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Scruffy.Data.Entity.Tables.CoreData;

namespace Scruffy.WebApp.Components.Account;

/// <summary>
/// This is a server-side AuthenticationStateProvider that revalidates the security stamp for the connected user every 30 minutes an interactive circuit is connected.
/// </summary>
internal sealed class IdentityRevalidatingAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    #region Fields

    /// <summary>
    /// Scope factory
    /// </summary>
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Options
    /// </summary>
    private readonly IOptions<IdentityOptions> _options;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="loggerFactory">Logger factory</param>
    /// <param name="scopeFactory">Scope factory</param>
    /// <param name="options">Options</param>
    public IdentityRevalidatingAuthenticationStateProvider(ILoggerFactory loggerFactory, IServiceScopeFactory scopeFactory, IOptions<IdentityOptions> options)
        : base(loggerFactory)
    {
        _scopeFactory = scopeFactory;
        _options = options;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Validate security stamp
    /// </summary>
    /// <param name="userManager">User manager</param>
    /// <param name="principal">Principal</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task<bool> ValidateSecurityStamp(UserManager<UserEntity> userManager, ClaimsPrincipal principal)
    {
        var user = await userManager.GetUserAsync(principal)
                                    .ConfigureAwait(false);

        if (user is null)
        {
            return false;
        }

        if (userManager.SupportsUserSecurityStamp == false)
        {
            return true;
        }

        var principalStamp = principal.FindFirstValue(_options.Value.ClaimsIdentity.SecurityStampClaimType);
        var userStamp = await userManager.GetSecurityStampAsync(user)
                                         .ConfigureAwait(false);

        return principalStamp == userStamp;
    }

    #endregion // Methods

    #region RevalidatingServerAuthenticationStateProvider

    /// <inheritdoc/>
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    /// <inheritdoc/>
    protected override async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        // Get the user manager from a new scope to ensure it fetches fresh data
        var scope = _scopeFactory.CreateAsyncScope();

        await using (scope.ConfigureAwait(false))
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserEntity>>();

            return await ValidateSecurityStamp(userManager, authenticationState.User).ConfigureAwait(false);
        }
    }

    #endregion // RevalidatingServerAuthenticationStateProvider
}