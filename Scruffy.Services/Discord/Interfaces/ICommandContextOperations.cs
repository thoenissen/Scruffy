namespace Scruffy.Services.Discord.Interfaces;

/// <summary>
/// Command context operations
/// </summary>
public interface ICommandContextOperations
{
    /// <summary>
    /// Show help of command
    /// </summary>
    /// <param name="commandName">Command</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ShowHelp(string commandName);
}