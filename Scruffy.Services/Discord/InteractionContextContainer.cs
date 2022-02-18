using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord.Interfaces;

namespace Scruffy.Services.Discord;

/// <summary>
/// Interaction context
/// </summary>
public sealed class InteractionContextContainer : IInteractionContext, IContextContainer, IDisposable
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="discordClient">Discord client</param>
    /// <param name="interaction">Interaction</param>
    public InteractionContextContainer(DiscordSocketClient discordClient, SocketInteraction interaction)
    {
        ServiceProvider = ServiceProviderContainer.Current.GetServiceProvider();
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

    #region Methods

    /// <summary>
    /// Response general processing message
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<IUserMessage> DeferProcessing()
    {
        await Interaction.RespondAsync(ServiceProvider.GetRequiredService<LocalizationService>()
                                                      .GetGroup(nameof(InteractionContextContainer))
                                                      .GetFormattedText("Processing",
                                                                        "{0} The action is being processed.",
                                                                        DiscordEmoteService.GetLoadingEmote(Client)))
                         .ConfigureAwait(false);

        return await Interaction.GetOriginalResponseAsync()
                                .ConfigureAwait(false);
    }

    #endregion // Methods

    #region IContextContainer

    /// <summary>
    /// Current member
    /// </summary>
    public IGuildUser Member => User as IGuildUser;

    /// <summary>
    /// Get merged context container
    /// </summary>
    /// <returns><see cref="MergedContextContainer"/>-Object</returns>
    public MergedContextContainer GetMergedContextContainer() => new (this);

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
    public async Task<IUserMessage> ReplyAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null)
    {
        if (stickers != null)
        {
            throw new NotSupportedException();
        }

        if (Interaction is SocketInteraction { HasResponded: false }
                        or RestInteraction { HasResponded: false })
        {
            await Interaction.RespondAsync(text, embeds, isTTS, false, allowedMentions, components, embed, options)
                             .ConfigureAwait(false);

            return await Interaction.GetOriginalResponseAsync()
                                    .ConfigureAwait(false);
        }

        return await Interaction.ModifyOriginalResponseAsync(obj =>
                                                             {
                                                                 if (text != null)
                                                                 {
                                                                     obj.Content = text;
                                                                 }

                                                                 if (embed != null)
                                                                 {
                                                                     obj.Embed = embed;
                                                                 }

                                                                 if (allowedMentions != null)
                                                                 {
                                                                     obj.AllowedMentions = allowedMentions;
                                                                 }

                                                                 if (components != null)
                                                                 {
                                                                     obj.Components = components;
                                                                 }

                                                                 if (embeds != null)
                                                                 {
                                                                     obj.Embeds = embeds;
                                                                 }
                                                             })
                                .ConfigureAwait(false);
    }

    /// <summary>
    /// Reply to the user message or command
    /// </summary>
    /// <param name="text">The message to be sent.</param>
    /// <param name="isTTS">Determines whether the message should be read aloud by Discord or not.</param>
    /// <param name="embed">The <see cref="EmbedType.Rich"/> <see cref="Embed"/> to be sent.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <param name="allowedMentions">Specifies if notifications are sent for mentioned users and roles in the message <paramref name="text"/>. If <c>null</c>, all mentioned roles and users will be notified./// </param>
    /// <param name="messageReference">The message references to be included. Used to reply to specific messages.</param>
    /// <param name="components">The message components to be included with this message. Used for interactions.</param>
    /// <param name="stickers">A collection of stickers to send with the message.</param>
    /// <param name="embeds">A array of <see cref="Embed"/>s to send with this response. Max 10.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<IUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null)
    {
        if (Interaction is SocketInteraction { HasResponded: false }
                        or RestInteraction { HasResponded: false })
        {
            if (stickers != null)
            {
                throw new NotSupportedException();
            }

            await Interaction.RespondAsync(text, embeds, isTTS, false, allowedMentions, components, embed, options)
                             .ConfigureAwait(false);

            return await Interaction.GetOriginalResponseAsync()
                                    .ConfigureAwait(false);
        }

        return await Channel.SendMessageAsync(text, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds)
                            .ConfigureAwait(false);
    }

    #endregion // IContextContainer

    #region IInteractionContext

    /// <summary>
    /// Gets the <see cref="T:Discord.DiscordSocketClient" /> that the command is executed with.
    /// </summary>
    public DiscordSocketClient Client { get; }

    /// <summary>
    /// Gets the <see cref="T:Discord.IDiscordClient" /> that the command is executed with.
    /// </summary>
    IDiscordClient IInteractionContext.Client => Client;

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