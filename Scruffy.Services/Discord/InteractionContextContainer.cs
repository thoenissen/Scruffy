using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core;
using Scruffy.Services.Core.Exceptions;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord.Interfaces;

namespace Scruffy.Services.Discord;

/// <summary>
/// Interaction context
/// </summary>
public sealed class InteractionContextContainer : IInteractionContext, IContextContainer, IDisposable
{
    #region Fields

    /// <summary>
    /// First followup message
    /// </summary>
    private IUserMessage _firstFollowup;

    /// <summary>
    /// Interaction
    /// </summary>
    private IDiscordInteraction _interaction;

    #endregion // Fields

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
        _interaction = interaction;
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

    /// <summary>
    /// Message
    /// </summary>
    public IMessage Message => (_interaction as IComponentInteraction)?.Message;

    #endregion Propeties

    #region Methods

    /// <summary>
    /// Response general processing message
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<IUserMessage> DeferProcessing()
    {
        try
        {
            await _interaction.RespondAsync(ServiceProvider.GetRequiredService<LocalizationService>()
                                                           .GetGroup(nameof(InteractionContextContainer))
                                                           .GetFormattedText("Processing",
                                                                             "{0} The action is being processed.",
                                                                             DiscordEmoteService.GetLoadingEmote(Client)))
                              .ConfigureAwait(false);

            return await _interaction.GetOriginalResponseAsync()
                                     .ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            throw new ScruffyAbortedException();
        }
    }

    /// <summary>
    /// Acknowledge the interaction
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DeferAsync()
    {
        try
        {
            await _interaction.DeferAsync()
                              .ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            throw new ScruffyAbortedException();
        }
    }

    /// <summary>
    /// Response with a modal
    /// </summary>
    /// <param name="customId">Custom ID</param>
    /// <typeparam name="TModal">Modal data</typeparam>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RespondWithModalAsync<TModal>(string customId)
        where TModal : class, IModal
    {
        try
        {
            await _interaction.RespondWithModalAsync<TModal>(customId)
                              .ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            throw new ScruffyAbortedException();
        }
    }

    #endregion // Methods

    #region IContextContainer

    /// <summary>
    /// Current member
    /// </summary>
    public IGuildUser Member => User as IGuildUser;

    /// <summary>
    /// Switching to a direct message context
    /// </summary>
    /// <returns>ICommandContext-implementation</returns>
    public async Task SwitchToDirectMessageContext()
    {
        if (Channel is not IDMChannel)
        {
            var dmChannel = await Member.CreateDMChannelAsync()
                                        .ConfigureAwait(false);

            Guild = null;
            Channel = dmChannel;
            User = dmChannel.Recipient;

            _interaction = null;
        }
    }

    /// <summary>
    /// Get merged context container
    /// </summary>
    /// <returns><see cref="MergedContextContainer"/>-Object</returns>
    public MergedContextContainer GetMergedContextContainer() => new(this);

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
    /// <param name="ephemeral">Should the message be posted ephemeral if possible?</param>
    /// <param name="attachments">File attachments</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<IUserMessage> ReplyAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, bool ephemeral = false, IEnumerable<FileAttachment> attachments = null)
    {
        try
        {
            if (_interaction != null)
            {
                if (stickers != null)
                {
                    throw new NotSupportedException();
                }

                if (_interaction.HasResponded == false)
                {
                    if (attachments == null)
                    {
                        await _interaction.RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options)
                                          .ConfigureAwait(false);
                    }
                    else
                    {
                        await _interaction.RespondWithFilesAsync(attachments, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options)
                                          .ConfigureAwait(false);
                    }

                    return await _interaction.GetOriginalResponseAsync()
                                             .ConfigureAwait(false);
                }

                if (_interaction is IComponentInteraction)
                {
                    if (_firstFollowup == null)
                    {
                        _firstFollowup = attachments != null
                                             ? await _interaction.FollowupAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options)
                                                                 .ConfigureAwait(false)
                                             : await _interaction.FollowupWithFilesAsync(attachments, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options)
                                                                 .ConfigureAwait(false);
                    }
                    else
                    {
                        await _firstFollowup.ModifyAsync(obj =>
                                                         {
                                                             obj.Content = text;
                                                             obj.Embed = embed;
                                                             obj.AllowedMentions = allowedMentions;
                                                             obj.Components = components;
                                                             obj.Embeds = embeds;
                                                             obj.Attachments = new Optional<IEnumerable<FileAttachment>>(attachments ?? Enumerable.Empty<FileAttachment>());
                                                         })
                                            .ConfigureAwait(false);

                        return _firstFollowup;
                    }
                }

                return await _interaction.ModifyOriginalResponseAsync(obj =>
                                                                      {
                                                                          if (text == null
                                                                           && embed == null)
                                                                          {
                                                                              obj.Content = "\u200b";
                                                                          }
                                                                          else
                                                                          {
                                                                              obj.Content = text;
                                                                              obj.Embed = embed;
                                                                          }

                                                                          obj.AllowedMentions = allowedMentions;
                                                                          obj.Components = components;
                                                                          obj.Embeds = embeds;
                                                                          obj.Attachments = new Optional<IEnumerable<FileAttachment>>(attachments ?? Enumerable.Empty<FileAttachment>());
                                                                      })
                                         .ConfigureAwait(false);
            }

            return attachments == null
                       ? await Channel.SendMessageAsync(text, isTTS, embed, options, allowedMentions, null, components, stickers, embeds)
                                      .ConfigureAwait(false)
                       : await Channel.SendFilesAsync(attachments, text, isTTS, embed, options, allowedMentions, null, components, stickers, embeds)
                                      .ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            throw new ScruffyAbortedException();
        }
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
    /// <param name="ephemeral">Should the message be posted ephemeral if possible?</param>
    /// <param name="attachments">File attachments</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<IUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, bool ephemeral = false, IEnumerable<FileAttachment> attachments = null)
    {
        try
        {
            if (_interaction != null)
            {
                if (_interaction.HasResponded == false)
                {
                    if (stickers != null)
                    {
                        throw new NotSupportedException();
                    }

                    if (attachments == null)
                    {
                        await _interaction.RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options)
                                          .ConfigureAwait(false);
                    }
                    else
                    {
                        await _interaction.RespondWithFilesAsync(attachments, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options)
                                          .ConfigureAwait(false);
                    }

                    return await _interaction.GetOriginalResponseAsync()
                                             .ConfigureAwait(false);
                }

                return attachments == null
                           ? await _interaction.FollowupAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options)
                                               .ConfigureAwait(false)
                           : await _interaction.FollowupWithFilesAsync(attachments, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options)
                                               .ConfigureAwait(false);
            }

            return attachments == null
                       ? await Channel.SendMessageAsync(text, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds)
                                      .ConfigureAwait(false)
                       : await Channel.SendFilesAsync(attachments, text, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds)
                                      .ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            throw new ScruffyAbortedException();
        }
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
    public IGuild Guild { get; private set; }

    /// <summary>
    /// Gets the channel the interaction originated from.
    /// </summary>
    public IMessageChannel Channel { get; private set; }

    /// <summary>
    /// Gets the user who invoked the interaction event.
    /// </summary>
    public IUser User { get; private set; }

    /// <summary>
    /// Gets the underlying interaction.
    /// </summary>
    IDiscordInteraction IInteractionContext.Interaction => _interaction;

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