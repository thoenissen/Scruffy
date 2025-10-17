namespace Scruffy.Services.Discord.Interfaces;

/// <summary>
/// Command context operations
/// </summary>
public interface ICommandContextOperations
{
    /// <summary>
    /// Show unmet precondition hint
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task ShowUnmetPrecondition();
}