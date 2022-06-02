using Discord;
using Discord.Rest;

using Microsoft.OpenApi.Models;

using Scruffy.Data.Entity;
using Scruffy.Data.Enumerations.General;
using Scruffy.Services.Core;

using Serilog;

namespace Scruffy.ServiceHosts.WebApi;

/// <summary>
/// Program
/// </summary>
public class Program
{
    /// <summary>
    /// Main method
    /// </summary>
    /// <param name="args">Arguments</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
                     .Enrich.FromLogContext()
                     .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                     .CreateBootstrapLogger();

        LoggingService.AddServiceLogEntry(LogEntryLevel.Information, nameof(Program), "Starting up", null);

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
                                           {
                                               c.SwaggerDoc("v1", new OpenApiInfo { Title = "Scruffy WebAPI", Version = "v1" });

                                               c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                                                                                 {
                                                                                     Name = "Authorization",
                                                                                     Type = SecuritySchemeType.ApiKey,
                                                                                     Scheme = "Bearer",
                                                                                     BearerFormat = "JWT",
                                                                                     In = ParameterLocation.Header,
                                                                                     Description = "JWT Authorization header using the Bearer scheme."
                                                                                 });
                                               c.AddSecurityRequirement(new OpenApiSecurityRequirement
                                                                        {
                                                                            {
                                                                                new OpenApiSecurityScheme
                                                                                {
                                                                                    Reference = new OpenApiReference
                                                                                                {
                                                                                                    Type = ReferenceType.SecurityScheme,
                                                                                                    Id = "Bearer"
                                                                                                }
                                                                                },
                                                                                new string[] { }
                                                                            }
                                                                        });
                                           });
#if !DEBUG
            builder.Services.AddAuthentication("Bearer")
                            .AddJwtBearer(options =>
                                          {
                                              options.Authority = Environment.GetEnvironmentVariable("SCRUFFY_AUTHORITY");
                                              options.TokenValidationParameters.ValidateAudience = false;
                                              options.RequireHttpsMetadata = false;
                                          });
            builder.Services.AddAuthorization(options => options.AddPolicy("ApiScope", policy =>
                                                                                       {
                                                                                           policy.RequireAuthenticatedUser();
                                                                                           policy.RequireClaim("scope", "api_v1");
                                                                                       }));
#endif

            var discordClient = new DiscordRestClient();

            await discordClient.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("SCRUFFY_DISCORD_TOKEN"))
                               .ConfigureAwait(false);

            builder.Services.AddSingleton(discordClient);
            builder.Services.AddSingleton<RepositoryFactory>();

            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseAuthentication();
            app.UseAuthorization();

#if DEBUG
            app.MapControllers();
#else
            app.MapControllers().RequireAuthorization("ApiScope");
#endif

            await app.RunAsync()
                     .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.CriticalError, nameof(Program), "Unhandled exception", null, ex);
        }
        finally
        {
            LoggingService.AddServiceLogEntry(LogEntryLevel.Information, nameof(Program), "Shut down complete", null);

            Log.CloseAndFlush();
        }
    }
}