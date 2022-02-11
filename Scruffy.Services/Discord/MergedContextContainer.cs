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

    #endregion // Properties

    #region IContextContainer

    /// <summary>
    /// Get merged context container
    /// </summary>
    /// <returns><see cref="MergedContextContainer"/>-Object</returns>
    public MergedContextContainer GetMergedContextContainer() => this;

    /// <summary>
    /// Reply to the user message or command
    /// </summary>
    /// <param name="text">The message to be sent.</param>
    /// <param name="isTTS">Determines whether the message should be read aloud by Discord or not.</param>
    /// <param name="embed">The <see cref="EmbedType.Rich"/> <see cref="Embed"/> to be sent.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <param name="allowedMentions">Specifies if notifications are sent for mentioned users and roles in the message <paramref name="text"/>. If <c>null</c>, all mentioned roles and users will be notified./// </param>
    /// <param name="components">The message components to be included with this message. Used for interactions.</param>
    /// <param name="stickers">A collection of stickers to send with the message.</param>
    /// <param name="embeds">A array of <see cref="Embed"/>s to send with this response. Max 10.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task<IUserMessage> ReplyAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null)
    {
        return _container.ReplyAsync(text, isTTS, embed, options, allowedMentions, components, stickers, embeds);
    }

    #endregion // IContextContainer
}