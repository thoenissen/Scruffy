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
public sealed class CommandContextContainer : ICommandContext, ICommandContextOperations, IDisposable
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
        ServiceProvider = GlobalServiceProvider.Current.GetServiceProvider();

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