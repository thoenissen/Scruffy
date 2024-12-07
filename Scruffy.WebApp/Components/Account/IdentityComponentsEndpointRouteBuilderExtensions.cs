using System;
using System.Collections.Generic;
using System.Security.Claims;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

using Scruffy.Data.Entity.Tables.CoreData;
using Scruffy.WebApp.Components.Account.Pages;

namespace Scruffy.WebApp.Components.Account;

/// <summary>
/// Extension methods to configure the web app
/// </summary>
internal static class IdentityComponentsEndpointRouteBuilderExtensions
{
    /// <summary>
    /// These endpoints are required by the Identity Razor components defined in the /Components/Account/Pages directory of this project.
    /// </summary>
    /// <param name="endpoints">Endpoint-Builder</param>
    public static void MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var accountGroup = endpoints.MapGroup("/Account");

        accountGroup.MapGet("/Login",
                            (HttpContext context, [FromServices] SignInManager<UserEntity> signInManager, [FromQuery] string returnUrl = null) =>
                            {
                                IEnumerable<KeyValuePair<string, StringValues>> query = [new("ReturnUrl", returnUrl), new("Action", ExternalLogin.LoginCallbackAction)];

                                var redirectUrl = UriHelper.BuildRelative(context.Request.PathBase, "/Account/ExternalLogin", QueryString.Create(query));
                                var properties = signInManager.ConfigureExternalAuthenticationProperties("Discord", redirectUrl);

                                return TypedResults.Challenge(properties, ["Discord"]);
                            });

        accountGroup.MapPost("/Logout",
                             async (ClaimsPrincipal user, SignInManager<UserEntity> signInManager, [FromForm] string returnUrl) =>
                             {
                                 await signInManager.SignOutAsync()
                                                    .ConfigureAwait(false);

                                 return TypedResults.LocalRedirect($"~/{returnUrl}");
                             });
    }
}