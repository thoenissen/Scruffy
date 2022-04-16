using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Scruffy.Services.Discord.Interfaces;

namespace Scruffy.Services.Discord;

/// <summary>
/// Implementation of <see cref="IInteractionContext"/> and <see cref="ICommandContext"/>
/// </summary>
public sealed class MergedContextContainer : IInteractionContext, ICommandContext, IContextContainer
{
    #region Fields

    /// <summary>
    /// Container
    /// </summary>
    private readonly IContextContainer _container;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="container">Container</param>
    public MergedContextContainer(IContextContainer container)
    {
        _container = container;

        Client = container.Client;
        ServiceProvider = container.ServiceProvider;
        Guild = container.Guild;
        Channel = container.Channel;
        User = container.User;
        Member = container.Member;

        if (container is CommandContextContainer commandContextContainer)
        {
            Message = commandContextContainer.Message;
        }
        else if (container is InteractionContextContainer interactionContextContainer)
        {
            Interaction = interactionContextContainer.Interaction;
        }
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Service provider
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Client
    /// </summary>
    public DiscordSocketClient Client { get; }

    /// <summary>
    /// Client
    /// </summary>
    IDiscordClient IInteractionContext.Client => Client;

    /// <summary>
    /// Client
    /// </summary>
    IDiscordClient ICommandContext.Client => Client;

    /// <summary>
    /// Guild
    /// </summary>
    public IGuild Guild { get; }

    /// <summary>
    /// Channel
    /// </summary>
    public IMessageChannel Channel { get; }

    /// <summary>
    /// user
    /// </summary>
    public IUser User { get; }

    /// <summary>
    /// Interaction
    /// </summary>
    public IDiscordInteraction Interaction { get; }

    /// <summary>
    /// Message
    /// </summary>
    public IUserMessage Message { get; }

    /// <summary>
    /// Member
    /// </summary>
    public IGuildUser Member { get; }

    /// <summary>
    /// Interactivity service
    /// </summary>
    public InteractivityService Interactivity => _container.Interactivity;

    #endregion // Properties

    #region IContextContainer

    /// <summary>
    /// Get merged context container
    /// </summary>
    /// <returns><see cref="MergedContextContainer"/>-Object</returns>
    public MergedContextContainer GetMergedContextContainer() => this;

    /// <inheritdoc/>
    public Task<IUserMessage> RespondAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
    {
        return _container.RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
    }

    /// <inheritdoc/>
    public Task<IUserMessage> SendMessageAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
    {
        return _container.SendMessageAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
    }

    #endregion // IContextContainer
}