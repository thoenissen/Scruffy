using Duende.IdentityServer;
using Duende.IdentityServer.Models;

using Microsoft.AspNetCore.HttpOverrides;

using Scruffy.Data.Entity;
using Scruffy.Data.Enumerations.General;
using Scruffy.Services.Core;

using Serilog;

namespace Scruffy.ServiceHosts.IdentityServer;

/// <summary>
/// Main class
/// </summary>
public class Program
{
    /// <summary>
    /// Main method
    /// </summary>
    /// <param name="args">Arguments</param>
    public static void Main(string[] args)
    {
        LoggingService.Initialize(config => config.Enrich.FromLogContext().CreateBootstrapLogger());
        LoggingService.AddServiceLogEntry(LogEntryLevel.Information, nameof(Program), "Starting up", null);

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog();

            builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost);
            builder.Services.AddRazorPages();
            builder.Services.AddIdentityServer(options =>
                                               {
                                                   options.Events.RaiseErrorEvents = true;
                                                   options.Events.RaiseInformationEvents = true;
                                                   options.Events.RaiseFailureEvents = true;
                                                   options.Events.RaiseSuccessEvents = true;
                                                   options.EmitStaticAudienceClaim = true;
                                               })
                            .AddInMemoryIdentityResources(new IdentityResource[]
                                                          {
                                                              new IdentityResources.OpenId(),
                                                              new IdentityResources.Profile(),
                                                          })
                            .AddInMemoryApiScopes(new ApiScope[]
                                                  {
                                                      new ("api_v1")
                                                  })
                            .AddInMemoryClients(new Client[]
                                                {
                                                    new ()
                                                    {
                                                        ClientId = Environment.GetEnvironmentVariable("SCRUFFY_WEBAPI_CLIENT_ID"),
                                                        ClientSecrets = { new Secret(Environment.GetEnvironmentVariable("SCRUFFY_WEBAPI_CLIENT_SECRET").Sha256()) },
                                                        AllowedGrantTypes = GrantTypes.Code,
                                                        RedirectUris = { Environment.GetEnvironmentVariable("SCRUFFY_WEBAPI_REDIRECT_URI") },
                                                        FrontChannelLogoutUri = Environment.GetEnvironmentVariable("SCRUFFY_WEBAPI_FRONT_CHANNEL_LOGOUT_URI"),
                                                        PostLogoutRedirectUris = { Environment.GetEnvironmentVariable("SCRUFFY_WEBAPI_POST_LOGOUT_REDIRECT_URI") },
                                                        AllowOfflineAccess = true,
                                                        AllowedScopes = { "openid", "profile", "api_v1" }
                                                    },
                                                });
            builder.Services.AddAuthentication()
                            .AddDiscord(options =>
                                        {
                                            options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                                            options.ClientId = Environment.GetEnvironmentVariable("SCRUFFY_DISCORD_OAUTH_CLIENT_ID");
                                            options.ClientSecret = Environment.GetEnvironmentVariable("SCRUFFY_DISCORD_OAUTH_CLIENT_SECRET");
                                        });
            builder.Services.AddScoped<RepositoryFactory>();

            var app = builder.Build();

            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();

            var forwardOptions = new ForwardedHeadersOptions
                                 {
                                     ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                                     RequireHeaderSymmetry = false,
                                 };

            forwardOptions.KnownNetworks.Clear();
            forwardOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(forwardOptions);
            app.UseIdentityServer();
            app.UseAuthorization();
            app.MapRazorPages()
               .RequireAuthorization();
            app.Run();
        }
        catch (Exception ex)
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.CriticalError, nameof(Program), "Unhandled exception", null, ex);
        }
        finally
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.Information, nameof(Program), "Shut down complete", null);
            LoggingService.CloseAndFlush();
        }
    }
}