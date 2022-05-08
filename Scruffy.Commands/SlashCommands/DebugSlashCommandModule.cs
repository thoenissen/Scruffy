using System.Collections.Concurrent;

using Discord;
using Discord.Interactions;

using Scruffy.Services.Discord;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Debugging commands
/// </summary>
[DontAutoRegister]
public class DebugSlashCommandModule : SlashCommandModuleBase
{
    #region Fields

    /// <summary>
    /// User ids
    /// </summary>
    private static ConcurrentDictionary<ulong, byte> _userIds;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    static DebugSlashCommandModule()
    {
        var userIds = Environment.GetEnvironmentVariable("SCRUFFY_DEVELOPER_USER_IDS") ?? string.Empty;

        _userIds = new ConcurrentDictionary<ulong, byte>(userIds.Split(";").ToDictionary(Convert.ToUInt64, obj => (byte)0));
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Debugging
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("debug", "Debugging")]
    public async Task Debug()
    {
        if (_userIds.ContainsKey(Context.User.Id))
        {
            await Context.DeferAsync()
                         .ConfigureAwait(false);
        }
        else
        {
            await Context.ReplyAsync(LocalizationGroup.GetText("MissingPermissions", "You don't have the required permissions to execute this command."))
                         .ConfigureAwait(false);
        }
    }

    #endregion // Methods
}