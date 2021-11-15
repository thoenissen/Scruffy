using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Scruffy.Services.Core.Discord.Attributes;

/// <summary>
/// Developer permissions
/// </summary>
public class RequireDeveloperPermissionsAttribute : CheckBaseAttribute
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
    static RequireDeveloperPermissionsAttribute()
    {
        var userIds = Environment.GetEnvironmentVariable("SCRUFFY_DEVELOPER_USER_IDS") ?? string.Empty;

        _userIds = new ConcurrentDictionary<ulong, byte>(userIds.Split(";").ToDictionary(Convert.ToUInt64, obj => (byte)0));
    }

    #endregion // Constructor

    #region CheckBaseAttribute

    /// <summary>
    /// Asynchronously checks whether this command can be executed within given context.
    /// </summary>
    /// <param name="ctx">Context to check execution ability for.</param>
    /// <param name="help">Whether this check is being executed from help or not. This can be used to probe whether command can be run without setting off certain fail conditions (such as cooldowns).</param>
    /// <returns>Whether the command can be executed in given context.</returns>
    public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        return Task.FromResult(_userIds.ContainsKey(ctx.User.Id));
    }

    #endregion // CheckBaseAttribute
}