using System.Collections.Immutable;

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
public sealed class InteractionContextContainer : IInteractionContext, IRouteMatchContainer, IContextContainer, IDisposable
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

    /// <summary>
    /// Defer processing message
    /// </summary>
    private IUserMessage _deferMessage;

    /// <summary>
    /// Matches
    /// </summary>
    private ImmutableArray<IRouteSegmentMatch> _segmentMatches;

    /// <summary>
    /// Original response message
    /// </summary>
    private IUserMessage _originalResponse;

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

        CustomId = (interaction.Data as IComponentInteractionData)?.CustomId;
        Client = discordClient;
        Guild = (interaction.User as IGuildUser)?.Guild;
        Channel = interaction.Channel;
        User = interaction.User;
        _interaction = interaction;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Custom Id
    /// </summary>
    public string CustomId { get; }

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

    /// <summary>
    /// Gets whether or not this interaction has been responded to
    /// </summary>
    public bool HasResponded => _interaction?.HasResponded == true;

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Response general processing message
    /// </summary>
    /// <param name="ephemeral">Ephemeral response</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<IUserMessage> DeferProcessing(bool ephemeral = false)
    {
        try
        {
            _deferMessage = await SendMessageAsync(ServiceProvider.GetRequiredService<LocalizationService>()
                                                                  .GetGroup(nameof(InteractionContextContainer))
                                                                  .GetFormattedText("Processing",
                                                                                    "{0} The action is being processed.",
                                                                                    DiscordEmoteService.GetLoadingEmote(Client)),
                                                                                    ephemeral: ephemeral).ConfigureAwait(false);

            return _deferMessage;
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
    /// <param name="modifyModal">Modal modification</param>
    /// <typeparam name="TModal">Modal data</typeparam>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RespondWithModalAsync<TModal>(string customId, Action<ModalBuilder> modifyModal = null)
        where TModal : class, IModal
    {
        try
        {
            await _interaction.RespondWithModalAsync<TModal>(customId, null, modifyModal)
                              .ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            throw new ScruffyAbortedException();
        }
    }

    /// <summary>
    /// Delete original response
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DeleteOriginalResponse()
    {
        try
        {
            await _interaction.DeleteOriginalResponseAsync()
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
            _deferMessage = null;

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
                        _firstFollowup = attachments == null
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
                                                             obj.Attachments = new Optional<IEnumerable<FileAttachment>>(attachments ?? []);
                                                         })
                                            .ConfigureAwait(false);
                    }

                    return _firstFollowup;
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
                                                                          obj.Attachments = new Optional<IEnumerable<FileAttachment>>(attachments ?? []);
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
                    _deferMessage = null;

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

                    _originalResponse = await _interaction.GetOriginalResponseAsync()
                                                          .ConfigureAwait(false);

                    return _originalResponse;
                }

                if (_deferMessage != null)
                {
                    var deferMessage = _deferMessage;

                    _deferMessage = null;

                    await deferMessage.ModifyAsync(obj =>
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
                                                       obj.Attachments = new Optional<IEnumerable<FileAttachment>>(attachments ?? []);
                                                   })
                                      .ConfigureAwait(false);

                    return deferMessage;
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

    /// <summary>
    /// Modify a message
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="func">Properties</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ModifyMessageAsync(IUserMessage message, Action<MessageProperties> func)
    {
        if (_interaction != null
            && message == _originalResponse)
        {
            await _interaction.ModifyOriginalResponseAsync(func)
                              .ConfigureAwait(false);
        }
        else
        {
            await message.ModifyAsync(func)
                         .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Delete messages
    /// </summary>
    /// <param name="messages">Messages</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DeleteMessages(List<IMessage> messages)
    {
        if (Channel is ITextChannel textChannel)
        {
            if (messages.Contains(_originalResponse))
            {
                await _interaction.DeleteOriginalResponseAsync()
                                  .ConfigureAwait(false);

                messages = messages.Except([_originalResponse])
                                   .ToList();
            }

            if (messages.Count > 0)
            {
                await textChannel.DeleteMessagesAsync(messages)
                                 .ConfigureAwait(false);
            }
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

    #region IRouteMatchContainer

    /// <summary>
    /// Sets the <see cref="P:Discord.IRouteMatchContainer.SegmentMatches" /> property of this container.
    /// </summary>
    /// <param name="segmentMatches">The collection of captured route segments.</param>
    void IRouteMatchContainer.SetSegmentMatches(IEnumerable<IRouteSegmentMatch> segmentMatches) => _segmentMatches = segmentMatches.ToImmutableArray();

    /// <summary>
    /// Gets the collection of captured route segments in this container.
    /// </summary>
    /// <returns>A collection of captured route segments.</returns>
    IEnumerable<IRouteSegmentMatch> IRouteMatchContainer.SegmentMatches => _segmentMatches;

    #endregion // IRouteMatchContainer
}