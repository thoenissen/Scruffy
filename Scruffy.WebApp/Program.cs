using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Tables.CoreData;
using Scruffy.Data.Entity.Tables.Web;
using Scruffy.WebApp.Components;
using Scruffy.WebApp.Components.Account;

namespace Scruffy.WebApp;

/// <summary>
/// Main application
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry
    /// </summary>
    public static void Main()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddRazorComponents()
                        .AddInteractiveServerComponents();
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IdentityRedirectManager>();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
        builder.Services.AddAuthentication(options =>
                                           {
                                               options.DefaultScheme = IdentityConstants.ApplicationScheme;
                                               options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                                           })
                        .AddDiscord(options =>
                                           {
                                               options.ClientId = Environment.GetEnvironmentVariable("SCRUFFY_DISCORD_OAUTH_CLIENT_ID")!;
                                               options.ClientSecret = Environment.GetEnvironmentVariable("SCRUFFY_DISCORD_OAUTH_CLIENT_SECRET")!;
                                           })
                        .AddIdentityCookies();
        builder.Services.AddDbContext<ScruffyDbContext>();
        builder.Services.AddQuickGridEntityFrameworkAdapter();
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddIdentityCore<UserEntity>()
                        .AddRoles<RoleEntity>()
                        .AddEntityFrameworkStores<ScruffyDbContext>()
                        .AddSignInManager()
                        .AddDefaultTokenProviders();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseForwardedHeaders(new ForwardedHeadersOptions
                                    {
                                        ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
                                    });
            app.UseHttpsRedirection();
        }

        app.UseStaticFiles();
        app.UseAntiforgery();
        app.MapRazorComponents<App>()
           .AddInteractiveServerRenderMode();
        app.MapAdditionalIdentityEndpoints();
        app.Run();
    }
}