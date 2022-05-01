using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.Guild;

namespace Scruffy.Commands.MessageComponents;

/// <summary>
/// Guild component commands
/// </summary>
public class GuildMessageComponentCommandModule : LocatedInteractionModuleBase
{
    #region Constants

    /// <summary>
    /// Group
    /// </summary>
    public const string Group = "guild";

    /// <summary>
    /// Command join
    /// </summary>
    public const string CommandNavigateToPageGuildRanking = "navigate_to_page_guild_ranking";

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public GuildCommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Navigate to the given page
    /// </summary>
    /// <param name="page">Page number</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [ComponentInteraction($"{Group};{CommandNavigateToPageGuildRanking};*;*")]
    public async Task NavigateToPageGuildRanking(int page)
    {
        await Context.DeferAsync()
                     .ConfigureAwait(false);

        await CommandHandler.NavigateToPageGuildRanking(Context, page)
                            .ConfigureAwait(false);
    }

    #endregion // Methods
}