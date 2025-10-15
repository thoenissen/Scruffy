using System.Threading;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core;

namespace Scruffy.Services.Discord;

/// <summary>
/// Managing the discord state of the bot
/// </summary>
public sealed class DiscordStatusService : SingletonLocatedServiceBase, IDisposable
{
    #region Fields

    /// <summary>
    /// Fields
    /// </summary>
    private DiscordSocketClient _discordClient;

    /// <summary>
    /// Timer
    /// </summary>
    private Timer _timer;

    /// <summary>
    /// Movie titles
    /// </summary>
    private Queue<string> _movies;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Status timer
    /// </summary>
    /// <param name="state">State</param>
    private void OnTimer(object state)
    {
        try
        {
            var title = _movies.Dequeue();

            _movies.Enqueue(title);

            Task.Run(async () => await _discordClient.SetActivityAsync(new Game(title, ActivityType.Watching)).ConfigureAwait(false));
        }
        catch
        {
        }
    }

    #endregion // Properties

    #region SingletonLocatedServiceBase

    /// <inheritdoc/>
    public override async Task Initialize(IServiceProvider serviceProvider)
    {
        await base.Initialize(serviceProvider)
                  .ConfigureAwait(false);

        _discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();
        _timer = new Timer(OnTimer, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(60));

        _movies = new Queue<string>(new[]
                                    {
                                        "The Amazing Spider-Quaggan",
                                        "The Quaggan of Balthazar High Road",
                                        "Interquaggan",
                                        "You, me and Peneloopee",
                                        "The Quaggan with the Icebrood tattoo",
                                        "Neon Quaggan Evangellion",
                                        "Game of Quaggans",
                                        "The Fast and the Quaggan",
                                        "Breath of the Quaggan 3",
                                        "The Lord of the Quaggan",
                                        "Pacific Quaggan",
                                        "Quaggan of the Dead",
                                        "Honey: We Quaggan'd ourselves",
                                        "My Neighbor Quaggan",
                                        "Into the Quaggan",
                                        "28 Quaggans later",
                                        "Quaggan begins",
                                        "Quaggan Reloaded",
                                        "Quaggan A.E.",
                                        "The last Quaggan",
                                        "Shaun the Quaggan",
                                        "Avengers Quaggangame",
                                        "Star Quaggan - The Revenge of the Fish",
                                        "The last Quagganhunter",
                                    });
    }

    #endregion // SingletonLocatedServiceBase

    #region IDisposable

    /// <summary>
    /// Dispose method
    /// </summary>
    public void Dispose()
    {
        _timer?.Dispose();
        _timer = null;
    }

    #endregion // IDisposable
}