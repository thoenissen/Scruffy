using Discord;
using Discord.WebSocket;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Core;

/// <summary>
/// Utility commands
/// </summary>
public class UtilityCommandHandler : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Discord client
    /// </summary>
    private readonly DiscordSocketClient _discordClient;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="discordClient">Discord client</param>
    public UtilityCommandHandler(LocalizationService localizationService, DiscordSocketClient discordClient)
        : base(localizationService)
    {
        _discordClient = discordClient;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Set the same reactions as of the given message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task AddReactions(IMessage message)
    {
        if (message.Channel is ITextChannel textChannel)
        {
            if (await textChannel.GetMessageAsync(message.Id).ConfigureAwait(false) is IUserMessage userMessage)
            {
                foreach (var reaction in userMessage.Reactions)
                {
                    await message.AddReactionAsync(reaction.Key)
                                 .ConfigureAwait(false);
                }
            }
        }
    }

    /// <summary>
    /// Remove the same reactions as of the given message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RemoveReactions(IMessage message)
    {
        if (message.Channel is ITextChannel textChannel)
        {
            if (await textChannel.GetMessageAsync(message.Id).ConfigureAwait(false) is IUserMessage userMessage)
            {
                foreach (var reaction in userMessage.Reactions)
                {
                    await message.RemoveReactionAsync(reaction.Key, _discordClient.CurrentUser)
                                 .ConfigureAwait(false);
                }
            }
        }
    }

    /// <summary>
    /// Repost message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RepostMessage(IUserMessage message)
    {
        var repostMessage = await message.Channel
                                         .SendMessageAsync(message.Content)
                                         .ConfigureAwait(false);

        foreach (var reaction in message.Reactions)
        {
            await repostMessage.AddReactionAsync(reaction.Key)
                               .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Add link to message
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="channelId">Channel id</param>
    /// <param name="messageId">Message id</param>
    /// <param name="name">Name</param>
    /// <param name="link">Link</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task AddLink(InteractionContextContainer context, ulong channelId, ulong messageId, string name, string link)
    {
        if (await context.Client.GetChannelAsync(channelId)
                         .ConfigureAwait(false) is ITextChannel channel)
        {
            if (await channel.GetMessageAsync(messageId)
                             .ConfigureAwait(false) is IUserMessage message)
            {
                var componentsBuilder = new ComponentBuilder();

                foreach (var component in message.Components)
                {
                    if (component is ActionRowComponent actionRow)
                    {
                        var rowBuilder = new ActionRowBuilder();

                        foreach (var innerComponent in actionRow.Components)
                        {
                            rowBuilder.AddComponent(innerComponent.ToBuilder());
                        }

                        componentsBuilder.AddRow(rowBuilder);
                    }
                }

                var actionRowBuilder = componentsBuilder.ActionRows?.LastOrDefault();

                if (actionRowBuilder == null)
                {
                    actionRowBuilder = new ActionRowBuilder();

                    componentsBuilder.AddRow(actionRowBuilder);
                }

                actionRowBuilder.WithButton(name, null, ButtonStyle.Link, null, link);

                await message.ModifyAsync(obj => obj.Components = componentsBuilder.Build())
                             .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Remove components
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RemoveComponents(IUserMessage message)
    {
        await message.ModifyAsync(obj => obj.Components = new ComponentBuilder().Build())
                     .ConfigureAwait(false);
    }

    #endregion // Methods
}