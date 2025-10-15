using Discord.Interactions;

using Scruffy.Data.Enumerations.Guild;
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

    /// <summary>
    /// Command join
    /// </summary>
    public const string CommandChangeTypeOfGuildRanking = "navigate_to_page_guild_ranking";

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
    /// <param name="unused">unused</param>
    /// <param name="pointTypeRaw">Point type</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [ComponentInteraction($"{Group};{CommandNavigateToPageGuildRanking};*;*;*")]
    public async Task NavigateToPageGuildRanking(int page, string unused, int? pointTypeRaw)
    {
        await Context.DeferAsync()
                     .ConfigureAwait(false);

        var pointType = default(GuildRankPointType?);

        if (pointTypeRaw >= 0
         && Enum.IsDefined(typeof(GuildRankPointType), pointTypeRaw.Value))
        {
            pointType = (GuildRankPointType)pointTypeRaw;
        }

        await CommandHandler.NavigateToPageGuildRanking(Context, page, pointType)
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Change the type
    /// </summary>
    /// <param name="type">Type</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [ComponentInteraction($"{Group};{CommandNavigateToPageGuildRanking};")]
    public async Task ChangeTypeOfGuildRanking(string type)
    {
        await Context.DeferAsync()
                     .ConfigureAwait(false);

        var pointType = default(GuildRankPointType?);

        if (int.TryParse(type, out var typeRaw))
        {
            if (typeRaw >= 0
             && Enum.IsDefined(typeof(GuildRankPointType), typeRaw))
            {
                pointType = (GuildRankPointType)typeRaw;
            }
        }

        await CommandHandler.NavigateToPageGuildRanking(Context, 0, pointType)
                            .ConfigureAwait(false);
    }

    #endregion // Methods
}