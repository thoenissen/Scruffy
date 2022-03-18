using Duende.IdentityServer;
using Duende.IdentityServer.Models;

using Scruffy.Data.Entity;

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
        Log.Logger = new LoggerConfiguration().WriteTo.Console()
                                              .CreateBootstrapLogger();

        Log.Information("Starting up");

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                                                   .Enrich.FromLogContext()
                                                   .ReadFrom.Configuration(ctx.Configuration));

            builder.Services.AddRazorPages();

            builder.Services
                   .AddIdentityServer(options =>
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

            builder.Services
                   .AddAuthentication()
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
            app.UseIdentityServer();
            app.UseAuthorization();

            app.MapRazorPages()
               .RequireAuthorization();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception");
        }
        finally
        {
            Log.Information("Shut down complete");
            Log.CloseAndFlush();
        }
    }
}