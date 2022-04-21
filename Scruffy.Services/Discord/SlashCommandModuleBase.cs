using Discord;
using Discord.Interactions;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core.Exceptions;

namespace Scruffy.Services.Discord;

/// <summary>
/// SlashCommand base class
/// </summary>
public abstract class SlashCommandModuleBase : LocatedInteractionModuleBase
{
    #region LocatedInteractionModuleBase

    /// <summary>
    /// Creates a list of all commands
    /// </summary>
    /// <remarks>Only the <see cref="SlashCommandBuildContext"/> is available and not the command context during this method.</remarks>
    /// <param name="buildContext">Build context</param>
    /// <returns>List of commands</returns>
    public virtual IEnumerable<ApplicationCommandProperties> GetCommands(SlashCommandBuildContext buildContext) => buildContext.ServiceProvider
                                                                                                                               .GetRequiredService<InteractionService>()
                                                                                                                               .Modules
                                                                                                                               .FirstOrDefault(obj => obj.Name == GetType().Name)
                                                                                                                               ?.ToApplicationCommandProps();

    #endregion // LocatedInteractionModuleBase

    #region InteractionModuleBase

    /// <summary>
    /// Before execution of the command
    /// </summary>
    /// <param name="command">Command</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task BeforeExecuteAsync(ICommandInfo command)
    {
        if (BlockedChannelService.Current.IsChannelBlocked(Context))
        {
            throw new ScruffyAbortedException();
        }

        await base.BeforeExecuteAsync(command)
                  .ConfigureAwait(false);
    }

    #endregion // InteractionModuleBase
}