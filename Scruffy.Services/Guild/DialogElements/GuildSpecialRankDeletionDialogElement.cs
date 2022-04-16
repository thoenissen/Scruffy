using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Deletion of a guild special rank
/// </summary>
public class GuildSpecialRankDeletionDialogElement : DialogReactionElementBase<bool>
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
    public GuildSpecialRankDeletionDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogReactionElementBase<bool>

    /// <summary>
    /// Editing the embedded message
    /// </summary>
    /// <returns>Message</returns>
    public override string GetMessage() => LocalizationGroup.GetText("DeletePrompt", "Are you sure you want to delete the special rank?");

    /// <summary>
    /// Returns the reactions which should be added to the message
    /// </summary>
    /// <returns>Reactions</returns>
    public override IReadOnlyList<ReactionData<bool>> GetReactions()
    {
        return _reactions ??= new List<ReactionData<bool>>
                              {
                                  new()
                                  {
                                      Emote = DiscordEmoteService.GetCheckEmote(CommandContext.Client),
                                      Func = () =>
                                             {
                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var rankId = DialogContext.GetValue<long>("RankId");

                                                     dbFactory.GetRepository<GuildSpecialRankConfigurationRepository>()
                                                              .Refresh(obj => obj.Id == rankId,
                                                                       obj => obj.IsDeleted = true);
                                                 }

                                                 return Task.FromResult(true);
                                             }
                                  },
                                  new()
                                  {
                                      Emote = DiscordEmoteService.GetCrossEmote(CommandContext.Client),
                                      Func = () => Task.FromResult(true)
                                  }
                              };
    }

    /// <summary>
    /// Default case if none of the given reactions is used
    /// </summary>
    /// <param name="reaction">Reaction</param>
    /// <returns>Result</returns>
    protected override bool DefaultFunc(IReaction reaction) => false;

    #endregion // DialogReactionElementBase<bool>
}