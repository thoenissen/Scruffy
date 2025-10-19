using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Interfaces;

namespace Scruffy.ManualTesting;

/// <summary>
/// Debugging context
/// </summary>
internal class DebuggingContext : IContextContainer
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    public DebuggingContext(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        Client = serviceProvider.GetRequiredService<DiscordSocketClient>();
        Interactivity = serviceProvider.GetRequiredService<InteractivityService>();
    }

    #endregion // Constructor

    #region Properties

    /// <inheritdoc/>
    public string CustomId => "Debug";

    /// <inheritdoc/>
    public IServiceProvider ServiceProvider { get; }

    /// <inheritdoc/>
    public DiscordSocketClient Client { get; }

    /// <inheritdoc/>
    public IGuild Guild { get; private set; }

    /// <inheritdoc/>
    public IMessageChannel Channel { get; private set; }

    /// <inheritdoc/>
    public IUser User { get; private set; }

    /// <inheritdoc/>
    public IGuildUser Member { get; private set; }

    /// <inheritdoc/>
    public InteractivityService Interactivity { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Initialize from a channel
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="userId">User ID</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InitializeFromChannel(ulong channelId, ulong userId)
    {
        Channel = await Client.GetChannelAsync(channelId).ConfigureAwait(false) as IMessageChannel;

        if (Channel is IGuildChannel guildChannel)
        {
            Guild = guildChannel.Guild;
            User = Member = await Guild.GetUserAsync(userId).ConfigureAwait(false);
        }
        else
        {
            User = await Client.GetUserAsync(userId).ConfigureAwait(false);
        }
    }

    #endregion // Methods

    #region IContextProvider

    /// <inheritdoc/>
    public Task SwitchToDirectMessageContext()
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IUserMessage> DeferProcessing(bool ephemeral = false)
    {
        return Task.FromResult<IUserMessage>(null);
    }

    /// <inheritdoc/>
    public async Task<IUserMessage> ReplyAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, bool ephemeral = false, IEnumerable<FileAttachment> attachments = null)
    {
        return await SendMessageAsync(text, isTTS, embed, options, allowedMentions, null, components, stickers, embeds, ephemeral, attachments).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, bool ephemeral = false, IEnumerable<FileAttachment> attachments = null)
    {
        return attachments == null
            ? await Channel.SendMessageAsync(text, isTTS, embed, options, allowedMentions, null, components, stickers, embeds)
                .ConfigureAwait(false)
            : await Channel.SendFilesAsync(attachments, text, isTTS, embed, options, allowedMentions, null, components, stickers, embeds)
                .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task ModifyMessageAsync(IUserMessage message, Action<MessageProperties> func)
    {
        var messageProperty = new MessageProperties();

        func(messageProperty);

        if (messageProperty.Attachments.IsSpecified)
        {
            await Channel.SendFilesAsync(messageProperty.Attachments.Value,
                                         messageProperty.Content.GetValueOrDefault(),
                                         false,
                                         messageProperty.Embed.GetValueOrDefault(),
                                         null,
                                         messageProperty.AllowedMentions.GetValueOrDefault(),
                                         null,
                                         messageProperty.Components.GetValueOrDefault(),
                                         null,
                                         messageProperty.Embeds.GetValueOrDefault(),
                                         messageProperty.Flags.GetValueOrDefault() ?? MessageFlags.None)
                         .ConfigureAwait(false);
        }
        else
        {
            await Channel.SendMessageAsync(messageProperty.Content.GetValueOrDefault(),
                                           false,
                                           messageProperty.Embed.GetValueOrDefault(),
                                           null,
                                           messageProperty.AllowedMentions.GetValueOrDefault(),
                                           null,
                                           messageProperty.Components.GetValueOrDefault(),
                                           null,
                                           messageProperty.Embeds.GetValueOrDefault(),
                                           messageProperty.Flags.GetValueOrDefault() ?? MessageFlags.None)
                         .ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public Task DeleteMessages(List<IMessage> messages)
    {
        return Task.CompletedTask;
    }

    #endregion // IContextProvider
}