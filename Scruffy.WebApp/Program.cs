using System;
using System.IO;
using System.Threading.Tasks;

using Discord;
using Discord.Rest;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Minio;
using Minio.AspNetCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Tables.CoreData;
using Scruffy.Data.Entity.Tables.Web;
using Scruffy.Services.Core.Localization;
using Scruffy.WebApp.Components;
using Scruffy.WebApp.Components.Account;
using Scruffy.WebApp.Services;

namespace Scruffy.WebApp;

/// <summary>
/// Main application
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task Main()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddHttpClient();
        builder.Services.AddRazorComponents()
                        .AddInteractiveServerComponents();
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IdentityRedirectManager>();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
        builder.Services.AddMinio(options =>
                                  {
                                      options.Endpoint = Environment.GetEnvironmentVariable("SCRUFFY_MINIO_ENDPOINT")!;
                                      options.ConfigureClient(client =>
                                                              {
                                                                  client.WithCredentials(Environment.GetEnvironmentVariable("SCRUFFY_MINIO_ACCESS_KEY")!,
                                                                                         Environment.GetEnvironmentVariable("SCRUFFY_MINIO_SECRET_KEY")!)
                                                                        .WithSSL(false)
                                                                        .WithRegion(Environment.GetEnvironmentVariable("SCRUFFY_MINIO_REGION"));
                                                              });
                                  });
        builder.Services.AddSingleton<DpsReportProcessor>();

        var locationService = new LocalizationService();

        builder.Services.AddSingleton(locationService);

        var discordClient = new DiscordRestClient();

        await discordClient.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("SCRUFFY_DISCORD_TOKEN")!)
                           .ConfigureAwait(false);

        builder.Services.AddSingleton(discordClient);

        var persistenceDirectory = Environment.GetEnvironmentVariable("SCRUFFY_PERSISTENCE_DIRECTORY");

        if (string.IsNullOrWhiteSpace(persistenceDirectory) == false)
        {
            builder.Services.AddDataProtection()
                            .PersistKeysToFileSystem(new DirectoryInfo(persistenceDirectory))
                            .SetApplicationName("Scruffy.WebApp");
        }

        builder.Services.AddAuthentication(options =>
                                           {
                                               options.DefaultScheme = IdentityConstants.ApplicationScheme;
                                               options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                                           })
                        .AddDiscord(options =>
                                           {
                                               options.ClientId = Environment.GetEnvironmentVariable("SCRUFFY_DISCORD_OAUTH_CLIENT_ID")!;
                                               options.ClientSecret = Environment.GetEnvironmentVariable("SCRUFFY_DISCORD_OAUTH_CLIENT_SECRET")!;
                                               options.Events.OnRemoteFailure = context =>
                                                                                {
                                                                                    context.Response.Redirect("/");
                                                                                    context.HandleResponse();

                                                                                    return Task.CompletedTask;
                                                                                };
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

        await locationService.Initialize(app.Services)
                             .ConfigureAwait(false);

        app.UseForwardedHeaders(new ForwardedHeadersOptions
                                {
                                    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
                                });

        if (app.Environment.IsDevelopment() == false)
        {
            app.UseHttpsRedirection();
        }

        app.UseRouting();
        app.UseStaticFiles();

        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();
        app.MapRazorComponents<App>()
           .AddInteractiveServerRenderMode();
        app.MapAdditionalIdentityEndpoints();
        await app.RunAsync()
                 .ConfigureAwait(false);
    }
}