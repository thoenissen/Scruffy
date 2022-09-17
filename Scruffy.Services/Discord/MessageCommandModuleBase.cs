using Discord;
using Discord.Interactions;

using Microsoft.Extensions.DependencyInjection;

namespace Scruffy.Services.Discord;

/// <summary>
/// MessageCommand base class
/// </summary>
public abstract class MessageCommandModuleBase : LocatedInteractionModuleBase
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
}