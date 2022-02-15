using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Services.CoreData;
using Scruffy.Services.Core;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord.Interfaces;

namespace Scruffy.Services.Discord;

/// <summary>
/// CommandContext wrapper
/// </summary>
public sealed class CommandContextContainer : ICommandContext, ICommandContextOperations, IContextContainer, IDisposable
{
    #region Fields

    /// <summary>
    /// User management
    /// </summary>
    private readonly UserManagementService _userManagementService;

    /// <summary>
    /// User data
    /// </summary>
    private UserData _userData;

    /// <summary>
    /// Original context
    /// </summary>
    private SocketCommandContext _commandContext;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="discordClient">Discord client</param>
    /// <param name="message">Message</param>
    public CommandContextContainer(DiscordSocketClient discordClient, SocketUserMessage message)
    {
        ServiceProvider = ServiceProviderContainer.Current.GetServiceProvider();

        _userManagementService = ServiceProvider.GetService<UserManagementService>();
        Interactivity = ServiceProvider.GetService<InteractivityService>();

        _commandContext = new SocketCommandContext(discordClient, message);

        Client = _commandContext.Client;
        Guild = _commandContext.Guild;
        Channel = _commandContext.Channel;
        User = _commandContext.User;
        Message = _commandContext.Message;
    }

    #endregion // Constructor

    #region Methods

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

            _commandContext = null;
        }
    }

    #endregion // Methods

    #region Properties

    /// <summary>
    /// Service provider
    /// </summary>
    public IServiceProvider ServiceProvider { get; private set; }

    /// <summary>
    /// Interactivity
    /// </summary>
    public InteractivityService Interactivity { get; }

    /// <summary>
    /// Current command
    /// </summary>
    public CommandInfo Command { get; internal set; }

    /// <summary>
    /// Operations
    /// </summary>
    public ICommandContextOperations Operations => this;

    /// <summary>
    /// Current member
    /// </summary>
    public IGuildUser Member => User as IGuildUser;

    #endregion // Properties

    #region Methods

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
    public Task<IUserMessage> ReplyAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null)
    {
        return Message.ReplyAsync(text, isTTS, embed, allowedMentions, options, components, stickers, embeds);
    }

    /// <summary>
    /// Get current user data
    /// </summary>
    /// <returns>Current user data</returns>
    public async Task<UserData> GetCurrentUser() => _userData ??= await _userManagementService.GetUserByDiscordAccountId(User.Id)
                                                                                              .ConfigureAwait(false);

    /// <summary>
    /// Show help of the given command
    /// </summary>
    /// <param name="commandName">Command name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    async Task ICommandContextOperations.ShowHelp(string commandName)
    {
        var helpService = ServiceProvider.GetService<CommandHelpService>();
        if (helpService != null)
        {
            await helpService.ShowHelp(this, commandName)
                             .ConfigureAwait(false);
        }
    }

    #endregion // Methods

    #region ICommandContext

    /// <summary>
    /// Gets the <see cref="T:Discord.DiscordSocketClient" /> that the command is executed with.
    /// </summary>
    public DiscordSocketClient Client { get; }

    /// <summary>
    /// Gets the <see cref="T:Discord.IDiscordClient" /> that the command is executed with.
    /// </summary>
    IDiscordClient ICommandContext.Client => Client;

    /// <summary>
    /// Gets the <see cref="T:Discord.IGuild" /> that the command is executed in.
    /// </summary>
    public IGuild Guild { get; private set; }

    /// <summary>
    /// Gets the <see cref="T:Discord.IMessageChannel" /> that the command is executed in.
    /// </summary>
    public IMessageChannel Channel { get; private set; }

    /// <summary>
    /// Gets the <see cref="T:Discord.IUser" /> who executed the command.
    /// </summary>
    public IUser User { get; private set; }

    /// <summary>
    /// Gets the <see cref="T:Discord.IUserMessage" /> that the command is interpreted from.
    /// </summary>
    public IUserMessage Message { get; }

    #endregion // ICommandContext

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