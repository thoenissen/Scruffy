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

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetText("DeletePrompt", "Are you sure you want to delete the special rank?");

    /// <inheritdoc/>
    public override IReadOnlyList<ReactionData<bool>> GetReactions()
    {
        return _reactions ??= [
                                  new ReactionData<bool>
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
                                  new ReactionData<bool>
                                  {
                                      Emote = DiscordEmoteService.GetCrossEmote(CommandContext.Client),
                                      Func = () => Task.FromResult(true)
                                  }
                              ];
    }

    /// <inheritdoc/>
    protected override bool DefaultFunc(IReaction reaction) => false;

    #endregion // DialogReactionElementBase<bool>
}