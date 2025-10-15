using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Tables.Guild;
using Scruffy.Data.Services.Guild;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.Guild.DialogElements.Forms;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Starting the guild special ranks assistant
/// </summary>
public class GuildSpecialRankSetupDialogElement : DialogEmbedReactionElementBase<bool>
{
    #region Fields

    /// <summary>
    /// Reactions
    /// </summary>
    private List<ReactionData<bool>> _reactions;

    /// <summary>
    /// Special ranks
    /// </summary>
    private List<GuildSpecialRankData> _ranks;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildSpecialRankSetupDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Returns the existing levels
    /// </summary>
    /// <returns>Levels</returns>
    private List<GuildSpecialRankData> GetRanks()
    {
        if (_ranks == null)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                _ranks = dbFactory.GetRepository<GuildSpecialRankConfigurationRepository>()
                                  .GetQuery()
                                  .Where(obj => obj.Guild.DiscordServerId == CommandContext.Guild.Id
                                             && obj.IsDeleted == false)
                                  .Select(obj => new GuildSpecialRankData
                                                 {
                                                     Id = obj.Id,
                                                     Description = obj.Description,
                                                     DiscordRoleId = obj.DiscordRoleId
                                                 })
                                  .ToList();
            }
        }

        return _ranks;
    }

    #endregion // Methods

    #region DialogReactionElementBase<bool>

    /// <inheritdoc/>
    public override Task EditMessage(EmbedBuilder builder)
    {
        builder.WithTitle(LocalizationGroup.GetText("ChooseCommandTitle", "Guild special rank configuration"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseCommandDescription", "With this assistant you are able to configure the guild special ranks. The following special ranks are already created:"));

        var levelsBuilder = new StringBuilder();

        var ranks = GetRanks();

        if (ranks.Count > 0)
        {
            foreach (var rank in ranks)
            {
                var role = CommandContext.Guild.GetRole(rank.DiscordRoleId);

                levelsBuilder.AppendLine(Format.Bold($"{rank.Description} ({role.Mention})"));
            }
        }
        else
        {
            levelsBuilder.Append('\u200B');
        }

        builder.AddField(LocalizationGroup.GetText("RanksFields", "Ranks"), levelsBuilder.ToString());

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override IReadOnlyList<ReactionData<bool>> GetReactions()
    {
        if (_reactions == null)
        {
            _reactions = new List<ReactionData<bool>>
                         {
                             new()
                             {
                                 Emote = DiscordEmoteService.GetAddEmote(CommandContext.Client),
                                 CommandText = LocalizationGroup.GetFormattedText("AddCommand", "{0} Add rank", DiscordEmoteService.GetAddEmote(CommandContext.Client)),
                                 Func = async () =>
                                        {
                                            var data = await DialogHandler.RunForm<CreateGuildSpecialRankData>(CommandContext, false)
                                                                          .ConfigureAwait(false);

                                            using (var dbFactory = RepositoryFactory.CreateInstance())
                                            {
                                                var rank = new GuildSpecialRankConfigurationEntity
                                                           {
                                                               GuildId = dbFactory.GetRepository<GuildRepository>()
                                                                                  .GetQuery()
                                                                                  .Where(obj => obj.DiscordServerId == CommandContext.Guild.Id)
                                                                                  .Select(obj => obj.Id)
                                                                                  .First(),
                                                               Description = data.Description,
                                                               DiscordRoleId = data.DiscordRoleId,
                                                               MaximumPoints = data.MaximumPoints,
                                                               GrantThreshold = data.GrantThreshold,
                                                               RemoveThreshold = data.RemoveThreshold
                                                           };

                                                if (dbFactory.GetRepository<GuildSpecialRankConfigurationRepository>()
                                                             .Add(rank))
                                                {
                                                    DialogContext.SetValue("RankId", rank.Id);

                                                    bool repeat;

                                                    do
                                                    {
                                                        repeat = await RunSubElement<GuildSpecialRankEditDialogElement, bool>().ConfigureAwait(false);
                                                    }
                                                    while (repeat);
                                                }
                                            }

                                            return true;
                                        }
                             }
                         };

            if (GetRanks().Count > 0)
            {
                _reactions.Add(new ReactionData<bool>
                               {
                                   Emote = DiscordEmoteService.GetEditEmote(CommandContext.Client),
                                   CommandText = LocalizationGroup.GetFormattedText("EditCommand", "{0} Edit rank", DiscordEmoteService.GetEditEmote(CommandContext.Client)),
                                   Func = async () =>
                                          {
                                              var levelId = await RunSubElement<GuildSpecialRankSelectionDialogElement, long>().ConfigureAwait(false);

                                              DialogContext.SetValue("RankId", levelId);

                                              bool repeat;

                                              do
                                              {
                                                  repeat = await RunSubElement<GuildSpecialRankEditDialogElement, bool>().ConfigureAwait(false);
                                              }
                                              while (repeat);

                                              return true;
                                          }
                               });

                _reactions.Add(new ReactionData<bool>
                               {
                                   Emote = DiscordEmoteService.GetTrashCanEmote(CommandContext.Client),
                                   CommandText = LocalizationGroup.GetFormattedText("DeleteCommand", "{0} Delete rank", DiscordEmoteService.GetTrashCanEmote(CommandContext.Client)),
                                   Func = async () =>
                                          {
                                              var rankId = await RunSubElement<GuildSpecialRankSelectionDialogElement, long>().ConfigureAwait(false);

                                              DialogContext.SetValue("RankId", rankId);

                                              return await RunSubElement<GuildSpecialRankDeletionDialogElement, bool>().ConfigureAwait(false);
                                          }
                               });
            }

            _reactions.Add(new ReactionData<bool>
                           {
                               Emote = DiscordEmoteService.GetCrossEmote(CommandContext.Client),
                               CommandText = LocalizationGroup.GetFormattedText("CancelCommand", "{0} Cancel", DiscordEmoteService.GetCrossEmote(CommandContext.Client)),
                               Func = () => Task.FromResult(false)
                           });
        }

        return _reactions;
    }

    /// <inheritdoc/>
    protected override string GetCommandTitle() => LocalizationGroup.GetText("CommandTitle", "Commands");

    /// <inheritdoc/>
    protected override bool DefaultFunc()
    {
        return false;
    }

    #endregion // DialogReactionElementBase<bool>
}