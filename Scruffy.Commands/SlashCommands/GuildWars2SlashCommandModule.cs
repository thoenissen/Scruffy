using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.GuildWars2;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Guild Wars 2 commands
/// </summary>
[Group("gw2", "Guild Wars 2 related commands")]
public class GuildWars2SlashCommandModule : SlashCommandModuleBase
{
    #region Enumerations

    /// <summary>
    /// Guide type
    /// </summary>
    public enum GuideType
    {
        [ChoiceDisplay("Raids")]
        Raids,
        [ChoiceDisplay("Strike Missions")]
        StrikeMissions,
        [ChoiceDisplay("Fractals of the Mists")]
        Fractals
    }

    #endregion // Enumerations

    #region Properties

    /// <summary>
    /// Command handler
    /// </summary>
    public GuildWars2CommandHandler CommandHandler { get; set; }

    #endregion // Properties

    #region Command methods

    /// <summary>
    /// Creation of a one time reminder
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("quaggan", "Posting random quaggan GIFs")]
    public Task Quaggan() => CommandHandler.Quaggan(Context);

    /// <summary>
    /// Next update
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("update", "Display information about the next Guild Wars 2 update")]
    public Task Update() => CommandHandler.Update(Context);

    /// <summary>
    /// Guides
    /// </summary>
    /// <param name="type">Type</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [SlashCommand("guides", "Guild Wars 2 guides")]
    public async Task Guides([Summary("Type", "Type of the guides which should be shown.")]GuideType type)
    {
        switch (type)
        {
            case GuideType.Raids:
                {
                    await CommandHandler.PostRaidGuides(Context)
                                        .ConfigureAwait(false);
                }
                break;
            case GuideType.StrikeMissions:
                {
                    await CommandHandler.PostStrikeMissionGuides(Context)
                                        .ConfigureAwait(false);
                }
                break;
            case GuideType.Fractals:
                {
                    await CommandHandler.PostFractalGuides(Context)
                                        .ConfigureAwait(false);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    #endregion // Command methods
}