using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// Deletion of a template
/// </summary>
public class RaidExperienceLevelDeletionDialogElement : DialogReactionElementBase<bool>
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
    public RaidExperienceLevelDeletionDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogReactionElementBase<bool>

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetText("DeleteLevelPrompt", "Are you sure you want to delete the level?");

    /// <inheritdoc/>
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
                                                     var levelId = DialogContext.GetValue<long>("ExperienceLevelId");

                                                     if (dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                                                  .Refresh(obj => obj.Id == levelId, obj => obj.IsDeleted = true))
                                                     {
                                                         dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                                                  .RefreshRanks();
                                                     }
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

    /// <inheritdoc/>
    protected override bool DefaultFunc(IReaction reaction) => false;

    #endregion // DialogReactionElementBase<bool>
}