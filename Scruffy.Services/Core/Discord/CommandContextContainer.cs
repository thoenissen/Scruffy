using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Scruffy.Data.Services.CoreData;
using Scruffy.Services.CoreData;

namespace Scruffy.Services.Core.Discord
{
    /// <summary>
    /// CommandContext wrapper
    /// </summary>
    public class CommandContextContainer
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
        private CommandContext _commandContext;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Create a wrapper from a normal CommandContext
        /// </summary>
        /// <param name="commandContext">CommandContext</param>
        /// <param name="userManagementService">User management service</param>
        /// <returns>ICommandContext-implementation</returns>
        public CommandContextContainer(CommandContext commandContext, UserManagementService userManagementService)
        {
            _commandContext = commandContext;
            _userManagementService = userManagementService;

            Client = commandContext.Client;
            CommandsNext = commandContext.CommandsNext;
            Prefix = commandContext.Prefix;

            Guild = commandContext.Guild;
            LastUserMessage = Message = commandContext.Message;
            Channel = commandContext.Channel;
            User = commandContext.User;
            Member = commandContext.Member;
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Switching to a direct message context
        /// </summary>
        /// <returns>ICommandContext-implementation</returns>
        public async Task SwitchToDirectMessageContext()
        {
            if (Channel.IsPrivate == false)
            {
                Channel = await Member.CreateDmChannelAsync()
                                      .ConfigureAwait(false);

                Member = null;

                _commandContext = null;
            }
        }

        #endregion // Methods

        #region Properties

        /// <summary>
        /// Current discord client
        /// </summary>
        public DiscordClient Client { get; private set; }

        /// <summary>
        /// This is the class which handles command registration, management, and execution.
        /// </summary>
        public CommandsNextExtension CommandsNext { get; private set; }

        /// <summary>
        /// Prefix
        /// </summary>
        public string Prefix { get; private set; }

        /// <summary>
        /// Current guild
        /// </summary>
        public DiscordGuild Guild { get; private set; }

        /// <summary>
        /// User message
        /// </summary>
        public DiscordMessage Message { get; private set; }

        /// <summary>
        /// Current channel
        /// </summary>
        public DiscordChannel Channel { get; private set; }

        /// <summary>
        /// Current user
        /// </summary>
        public DiscordUser User { get; private set; }

        /// <summary>
        /// Current member
        /// </summary>
        public DiscordMember Member { get; private set; }

        /// <summary>
        /// Last user message
        /// </summary>
        public DiscordMessage LastUserMessage { get; internal set; }

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
        public async Task ShowHelp(string commandName)
        {
            var cmd = CommandsNext.FindCommand("help " + commandName, out var customArgs);
            if (cmd != null)
            {
                var fakeContext = CommandsNext.CreateFakeContext(Member ?? User, Channel, "help " + commandName, Prefix, cmd, customArgs);

                await CommandsNext.ExecuteCommandAsync(fakeContext)
                                  .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Get the original context
        /// </summary>
        /// <returns>Original context</returns>
        public CommandContext GetCommandContext() => _commandContext;

        #endregion // Methods
    }
}