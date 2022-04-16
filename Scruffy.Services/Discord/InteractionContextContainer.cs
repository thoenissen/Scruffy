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
    /// <param name="ephemeral">Whether the response is ephemeral</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<IUserMessage> DeferProcessing(bool ephemeral = false)
    {
        await Interaction.RespondAsync(ServiceProvider.GetRequiredService<LocalizationService>()
                                                      .GetGroup(nameof(InteractionContextContainer))
                                                      .GetFormattedText("Processing",
                                                                        "{0} The action is being processed.",
                                                                        DiscordEmoteService.GetLoadingEmote(Client)),
                                                                        ephemeral: ephemeral)
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
    public MergedContextContainer GetMergedContextContainer() => new(this);

    /// <inheritdoc/>
    public async Task<IUserMessage> RespondAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
    {
        if (Interaction is SocketInteraction { HasResponded: false }
                        or RestInteraction { HasResponded: false })
        {
            await Interaction.RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options)
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

    /// <inheritdoc/>
    public async Task<IUserMessage> SendMessageAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
    {
        if (Interaction is SocketInteraction { HasResponded: false }
                        or RestInteraction { HasResponded: false })
        {
            await Interaction.RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options)
                             .ConfigureAwait(false);

            return await Interaction.GetOriginalResponseAsync()
                                    .ConfigureAwait(false);
        }

        return await Channel.SendMessageAsync(text: text, embeds: embeds, isTTS: isTTS, allowedMentions: allowedMentions, components: components, embed: embed, options: options)
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