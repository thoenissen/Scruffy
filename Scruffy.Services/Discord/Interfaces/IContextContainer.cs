﻿using Discord;
using Discord.WebSocket;

namespace Scruffy.Services.Discord.Interfaces;

/// <summary>
/// Context container
/// </summary>
public interface IContextContainer
{
    #region Properties

    /// <summary>
    /// Service provider
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the <see cref="T:Discord.DiscordSocketClient" /> that the command is executed with.
    /// </summary>
    DiscordSocketClient Client { get; }

    /// <summary>
    /// Gets the <see cref="T:Discord.IGuild" /> that the command is executed in.
    /// </summary>
    IGuild Guild { get; }

    /// <summary>
    /// Gets the <see cref="T:Discord.IMessageChannel" /> that the command is executed in.
    /// </summary>
    IMessageChannel Channel { get; }

    /// <summary>
    /// Gets the <see cref="T:Discord.IUser" /> who executed the command.
    /// </summary>
    IUser User { get; }

    /// <summary>
    /// Gets the <see cref="T:Discord.IGuildUser" /> who executed the command.
    /// </summary>
    IGuildUser Member { get; }

    /// <summary>
    /// Interactivity service
    /// </summary>
    InteractivityService Interactivity { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Get merged context container
    /// </summary>
    /// <returns><see cref="MergedContextContainer"/>-Object</returns>
    public MergedContextContainer GetMergedContextContainer();

    /// <summary>
    /// Reply to the user message or command
    /// </summary>
    /// <param name="text">The message to be sent.</param>
    /// <param name="embeds">A array of <see cref="Embed"/>s to send with this response. Max 10.</param>
    /// <param name="isTTS">Determines whether the message should be read aloud by Discord or not.</param>
    /// <param name="ephemeral">Whether all user can see the message or only the sender.</param>
    /// <param name="allowedMentions">Specifies if notifications are sent for mentioned users and roles in the message <paramref name="text"/>. If <c>null</c>, all mentioned roles and users will be notified./// </param>
    /// <param name="components">The message components to be included with this message. Used for interactions.</param>
    /// <param name="embed">The <see cref="EmbedType.Rich"/> <see cref="Embed"/> to be sent.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<IUserMessage> RespondAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null);

    /// <summary>
    /// Reply to the user message or command
    /// </summary>
    /// <param name="text">The message to be sent.</param>
    /// <param name="embeds">A array of <see cref="Embed"/>s to send with this response. Max 10.</param>
    /// <param name="isTTS">Determines whether the message should be read aloud by Discord or not.</param>
    /// <param name="ephemeral">Whether all user can see the message or only the sender.</param>
    /// <param name="allowedMentions">Specifies if notifications are sent for mentioned users and roles in the message <paramref name="text"/>. If <c>null</c>, all mentioned roles and users will be notified./// </param>
    /// <param name="components">The message components to be included with this message. Used for interactions.</param>
    /// <param name="embed">The <see cref="EmbedType.Rich"/> <see cref="Embed"/> to be sent.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<IUserMessage> SendMessageAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null);

    #endregion // Methods
}