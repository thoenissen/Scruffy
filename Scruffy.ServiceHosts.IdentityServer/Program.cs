using Duende.IdentityServer;
using Duende.IdentityServer.Models;

using Microsoft.AspNetCore.HttpOverrides;

using Scruffy.Data.Entity;
using Scruffy.ServiceHosts.IdentityServer.Services;

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
        Log.Logger = new LoggerConfiguration()
                     .Enrich.FromLogContext()
                     .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                     .CreateBootstrapLogger();

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
                                                      new("api_v1")
                                                  })
                            .AddInMemoryClients(new Client[]
                                                {
                                                    new()
                                                    {
                                                        ClientId = Environment.GetEnvironmentVariable("SCRUFFY_WEBAPI_CLIENT_ID"),
                                                        ClientSecrets = { new Secret(Environment.GetEnvironmentVariable("SCRUFFY_WEBAPI_CLIENT_SECRET").Sha256()) },
                                                        AllowedGrantTypes = GrantTypes.ClientCredentials,
                                                        AllowOfflineAccess = true,
                                                        AllowedScopes = { "openid", "profile", "api_v1" }
                                                    },
                                                    new()
                                                    {
                                                        ClientId = Environment.GetEnvironmentVariable("SCRUFFY_WEBAPP_CLIENT_ID"),
                                                        ClientSecrets = { new Secret(Environment.GetEnvironmentVariable("SCRUFFY_WEBAPP_CLIENT_SECRET").Sha256()) },
                                                        AllowedGrantTypes = GrantTypes.Code,
                                                        RedirectUris = { Environment.GetEnvironmentVariable("SCRUFFY_WEBAPP_REDIRECT_URI") },
                                                        AllowOfflineAccess = true,
                                                        AllowedScopes = { "openid", "profile", "api_v1" },
                                                        AllowedCorsOrigins = new[] { Environment.GetEnvironmentVariable("SCRUFFY_WEBAPP_CORS_ORIGINS") }
                                                    }
                                                });

            builder.Services.AddAuthentication()
                            .AddDiscord(options =>
                                        {
                                            options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                                            options.ClientId = Environment.GetEnvironmentVariable("SCRUFFY_DISCORD_OAUTH_CLIENT_ID");
                                            options.ClientSecret = Environment.GetEnvironmentVariable("SCRUFFY_DISCORD_OAUTH_CLIENT_SECRET");
                                        });

            builder.Services.AddScoped<RepositoryFactory>();
            builder.Services.AddSingleton<ProfileService>();

            var app = builder.Build();

            app.UseCookiePolicy(new CookiePolicyOptions
                                {
                                    MinimumSameSitePolicy = SameSiteMode.None
                                });

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
            Log.Error(ex, "Unhandled exception");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}