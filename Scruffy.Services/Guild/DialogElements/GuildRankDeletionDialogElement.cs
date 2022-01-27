
using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Deletion of a guild special rank
/// </summary>
public class GuildRankDeletionDialogElement : DialogReactionElementBase<bool>
{
    #region Fields

    /// <summary>
    /// Reactions
    /// </summary>
    private List<ReactionData<bool>> _reactions;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildRankDeletionDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogReactionElementBase<bool>

    /// <summary>
    /// Editing the embedded message
    /// </summary>
    /// <returns>Message</returns>
    public override string GetMessage() => LocalizationGroup.GetText("DeletePrompt", "Are you sure you want to delete the rank?");

    /// <summary>
    /// Returns the reactions which should be added to the message
    /// </summary>
    /// <returns>Reactions</returns>
    public override IReadOnlyList<ReactionData<bool>> GetReactions()
    {
        return _reactions ??= new List<ReactionData<bool>>
                              {
                                  new ()
                                  {
                                      Emoji = DiscordEmojiService.GetCheckEmoji(CommandContext.Client),
                                      Func = () =>
                                             {
                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var rankId = DialogContext.GetValue<int>("RankId");

                                                     if (dbFactory.GetRepository<GuildRankRepository>()
                                                                  .Remove(obj => obj.Id == rankId))
                                                     {
                                                         var order = 0;

                                                         foreach (var currentRankId in dbFactory.GetRepository<GuildRankRepository>()
                                                                                         .GetQuery()
                                                                                         .OrderBy(obj => obj.Order)
                                                                                         .Select(obj => obj.Id))
                                                         {
                                                             dbFactory.GetRepository<GuildRankRepository>()
                                                                      .Refresh(obj => obj.Id == currentRankId,
                                                                               obj => obj.Order = order);
                                                             order++;
                                                         }
                                                     }
                                                 }

                                                 return Task.FromResult(true);
                                             }
                                  },
                                  new ()
                                  {
                                      Emoji = DiscordEmojiService.GetCrossEmoji(CommandContext.Client),
                                      Func = () => Task.FromResult(true)
                                  }
                              };
    }

    /// <summary>
    /// Default case if none of the given reactions is used
    /// </summary>
    /// <param name="reaction">Reaction</param>
    /// <returns>Result</returns>
    protected override bool DefaultFunc(MessageReactionAddEventArgs reaction) => false;

    #endregion // DialogReactionElementBase<bool>
}