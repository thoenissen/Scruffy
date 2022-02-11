using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core;

namespace Scruffy.Services.Discord;

/// <summary>
/// Interaction context
/// </summary>
public sealed class InteractionContextContainer : IInteractionContext, IDisposable
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="discordClient">Discord client</param>
    /// <param name="interaction">Interaction</param>
    public InteractionContextContainer(IDiscordClient discordClient, SocketInteraction interaction)
    {
        ServiceProvider = GlobalServiceProvider.Current.GetServiceProvider();
        Interactivity = ServiceProvider.GetService<InteractivityService>();

        Client = discordClient;
        Guild = (interaction.User as IGuildUser)?.Guild;
        Channel = interaction.Channel;
        User = interaction.User;
        Interaction = interaction;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Service provider
    /// </summary>
    public IServiceProvider ServiceProvider { get; set; }

    /// <summary>
    /// Interactivity
    /// </summary>
    public InteractivityService Interactivity { get; }

    /// <summary>
    /// Command
    /// </summary>
    public ICommandInfo Command { get; internal set; }

    #endregion Propeties

    #region IInteractionContext

    /// <summary>
    /// Gets the client that will be used to handle this interaction.
    /// </summary>
    public IDiscordClient Client { get; }

    /// <summary>
    /// Gets the guild the interaction originated from.
    /// </summary>
    /// <remarks> Will be <see langword="null" /> if the interaction originated from a DM channel or the interaction was a Context Command interaction. </remarks>
    public IGuild Guild { get; }

    /// <summary>
    /// Gets the channel the interaction originated from.
    /// </summary>
    public IMessageChannel Channel { get; }

    /// <summary>
    /// Gets the user who invoked the interaction event.
    /// </summary>
    public IUser User { get; }

    /// <summary>
    /// Gets the underlying interaction.
    /// </summary>
    public IDiscordInteraction Interaction { get; }

    #endregion // IInteractionContext

    #region IDisposable

    /// <summary>
    /// Dispose method
    /// </summary>
    public void Dispose()
    {
        (ServiceProvider as ServiceProvider)?.Dispose();
        ServiceProvider = null;
    }

    #endregion // IDisposable
}