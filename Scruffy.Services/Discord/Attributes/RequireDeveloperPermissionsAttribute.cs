using System.Collections.Concurrent;

using Discord.Commands;

using Scruffy.Services.Core;

namespace Scruffy.Services.Discord.Attributes;

/// <summary>
/// Developer permissions
/// </summary>
public class RequireDeveloperPermissionsAttribute : PreconditionAttribute
{
    #region Fields

    /// <summary>
    /// User ids
    /// </summary>
    private ConcurrentDictionary<ulong, byte> _userIds;

    #endregion // Fields

    #region PreconditionAttribute

    /// <inheritdoc/>
    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        if (_userIds == null)
        {
            var userIds = ConfigurationService.GetEntry("SCRUFFY_DEVELOPER_USER_IDS") ?? string.Empty;

            _userIds = new ConcurrentDictionary<ulong, byte>(userIds.Split(";").ToDictionary(Convert.ToUInt64, obj => (byte)0));
        }

        return Task.FromResult(_userIds.ContainsKey(context.User.Id)
                                   ? PreconditionResult.FromSuccess()
                                   : PreconditionResult.FromError("The channel is blocked for commands."));
    }

    #endregion // PreconditionAttribute
}