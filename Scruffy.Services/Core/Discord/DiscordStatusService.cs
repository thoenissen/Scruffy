using System;
using System.Collections.Generic;
using System.Threading;

using DSharpPlus;
using DSharpPlus.Entities;

namespace Scruffy.Services.Core.Discord;

/// <summary>
/// Managing the discord state of the bot
/// </summary>
public sealed class DiscordStatusService : IDisposable
{
    #region Fields

    /// <summary>
    /// Fields
    /// </summary>
    private DiscordClient _discordClient;

    /// <summary>
    /// Timer
    /// </summary>
    private Timer _timer;

    /// <summary>
    /// Movie titles
    /// </summary>
    private Queue<string> _movies;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="discordClient">Client</param>
    public DiscordStatusService(DiscordClient discordClient)
    {
        _discordClient = discordClient;
        _timer = new Timer(OnTimer, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(60));

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

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Status timer
    /// </summary>
    /// <param name="state">State</param>
    private async void OnTimer(object state)
    {
        try
        {
            var title = _movies.Dequeue();
            _movies.Enqueue(title);

            await _discordClient.UpdateStatusAsync(new DiscordActivity(title, ActivityType.Watching)).ConfigureAwait(false);
        }
        catch
        {
        }
    }

    #endregion // Properties

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