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
    /// Command
    /// </summary>
    public ICommandInfo Command { get; internal set; }

    /// <summary>
    /// Message
    /// </summary>
    public IMessage Message => (_interaction as IComponentInteraction)?.Message;

    /// <summary>
    /// Has this interaction been responded to?
    /// </summary>
    public bool HasResponded => _interaction?.HasResponded == true;

    /// <summary>
    /// Is the response ephemeral?
    /// </summary>
    public bool IsEphemeralResponse { get; private set; } = true;

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Acknowledge the interaction
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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
    /// <typeparam name="TModal">Modal data</typeparam>
    /// <param name="customId">Custom ID</param>
    /// <param name="modifyModal">Modal modification</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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

    /// <inheritdoc/>
    public string CustomId { get; }

    /// <inheritdoc/>
    public IServiceProvider ServiceProvider { get; set; }

    /// <inheritdoc/>
    public InteractivityService Interactivity { get; }

    /// <inheritdoc/>
    public DiscordSocketClient Client { get; }

    /// <inheritdoc/>
    public IGuildUser Member => User as IGuildUser;

    /// <inheritdoc/>
    public async Task<IUserMessage> DeferProcessing(bool ephemeral = false)
    {
        try
        {
            IsEphemeralResponse = ephemeral;

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

    /// <inheritdoc/>
    public async Task<IUserMessage> DeferProcessing(string title, string message, string footer)
    {
        try
        {
            var components = new ComponentBuilderV2().WithContainer(container => container.WithTextDisplay($"# {title}")
                                                                                          .WithSeparator()
                                                                                          .WithTextDisplay($"{DiscordEmoteService.GetLoadingEmote(Client)} {message}")
                                                                                          .WithSeparator()
                                                                                          .WithTextDisplay($"-# {footer}")
                                                                                          .WithAccentColor(Color.DarkPurple))
                                                     .Build();

            _deferMessage = await SendMessageAsync(components: components).ConfigureAwait(false);

            return _deferMessage;
        }
        catch (TimeoutException)
        {
            throw new ScruffyAbortedException();
        }
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<IUserMessage> ReplyAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, bool ephemeral = false, IEnumerable<FileAttachment> attachments = null)
    {
        try
        {
            IsEphemeralResponse = ephemeral;

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
                                                                              if (components == null
                                                                                  || components.Components.Any(component => component.Type == ComponentType.Container) == false)
                                                                              {
                                                                                  obj.Content = "\u200b";
                                                                              }
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

    /// <inheritdoc/>
    public async Task<IUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, bool ephemeral = false, IEnumerable<FileAttachment> attachments = null)
    {
        try
        {
            IsEphemeralResponse = ephemeral;

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
                                                           if (components == null
                                                               || components.Components.Any(component => component.Type == ComponentType.Container) == false)
                                                           {
                                                               obj.Content = "\u200b";
                                                           }
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    IDiscordClient IInteractionContext.Client => Client;

    /// <inheritdoc/>
    public IGuild Guild { get; private set; }

    /// <inheritdoc/>
    public IMessageChannel Channel { get; private set; }

    /// <inheritdoc/>
    public IUser User { get; private set; }

    /// <inheritdoc/>
    IDiscordInteraction IInteractionContext.Interaction => _interaction;

    #endregion // IInteractionContext

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose()
    {
        (ServiceProvider as ServiceProvider)?.Dispose();
        ServiceProvider = null;
    }

    #endregion // IDisposable

    #region IRouteMatchContainer

    /// <inheritdoc/>
    void IRouteMatchContainer.SetSegmentMatches(IEnumerable<IRouteSegmentMatch> segmentMatches)
    {
        _segmentMatches = segmentMatches.ToImmutableArray();
    }

    /// <inheritdoc/>
    IEnumerable<IRouteSegmentMatch> IRouteMatchContainer.SegmentMatches => _segmentMatches;

    #endregion // IRouteMatchContainer
}