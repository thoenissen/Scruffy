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
/// Guild rank setup
/// </summary>
public class GuildRankSetupDialogElement : DialogEmbedReactionElementBase<bool>
{
    #region Fields

    /// <summary>
    /// Reactions
    /// </summary>
    private List<ReactionData<bool>> _reactions;

    /// <summary>
    /// Special ranks
    /// </summary>
    private List<GuildRankData> _ranks;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildRankSetupDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Returns the existing levels
    /// </summary>
    /// <returns>Levels</returns>
    private List<GuildRankData> GetRanks()
    {
        if (_ranks == null)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                _ranks = [];

                var ranks = dbFactory.GetRepository<GuildRankRepository>()
                                     .GetQuery()
                                     .Where(obj => obj.Guild.DiscordServerId == CommandContext.Guild.Id)
                                     .OrderBy(obj => obj.Order)
                                     .Select(obj => new
                                                    {
                                                        obj.Id,
                                                        obj.InGameName,
                                                        obj.DiscordRoleId
                                                    })
                                     .ToList();

                foreach (var rank in ranks)
                {
                    _ranks.Add(new GuildRankData
                               {
                                   Id = rank.Id,
                                   InGameName = rank.InGameName,
                                   DiscordRoleId = rank.DiscordRoleId
                               });
                }
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
                levelsBuilder.AppendLine(Format.Bold($"{rank.InGameName} ({CommandContext.Guild.GetRole(rank.DiscordRoleId).Mention})"));
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
            _reactions = [
                             new ReactionData<bool>
                             {
                                 Emote = DiscordEmoteService.GetAddEmote(CommandContext.Client),
                                 CommandText = LocalizationGroup.GetFormattedText("AddCommand", "{0} Add rank", DiscordEmoteService.GetAddEmote(CommandContext.Client)),
                                 Func = async () =>
                                        {
                                            var data = await DialogHandler.RunForm<CreateGuildRankFormData>(CommandContext, false)
                                                                          .ConfigureAwait(false);

                                            using (var dbFactory = RepositoryFactory.CreateInstance())
                                            {
                                                var rank = new GuildRankEntity
                                                           {
                                                               GuildId = dbFactory.GetRepository<GuildRepository>()
                                                                                  .GetQuery()
                                                                                  .Where(obj => obj.DiscordServerId == CommandContext.Guild.Id)
                                                                                  .Select(obj => obj.Id)
                                                                                  .First(),
                                                               DiscordRoleId = data.DiscordRoleId,
                                                               InGameName = data.InGameName,
                                                               Percentage = data.Percentage,
                                                               Order = data.SuperiorId != null
                                                                           ? dbFactory.GetRepository<GuildRankRepository>()
                                                                                      .GetQuery()
                                                                                      .Where(obj => obj.Id == data.SuperiorId)
                                                                                      .Select(obj => obj.Order)
                                                                                      .First() + 1
                                                                           : 0,
                                                           };

                                                if (dbFactory.GetRepository<GuildRankRepository>()
                                                             .Add(rank))
                                                {
                                                    var order = rank.Order + 1;

                                                    foreach (var currentRankId in dbFactory.GetRepository<GuildRankRepository>()
                                                                                           .GetQuery()
                                                                                           .Where(obj => obj.Order >= rank.Order)
                                                                                           .OrderBy(obj => obj.Order)
                                                                                           .Select(obj => obj.Id))
                                                    {
                                                        dbFactory.GetRepository<GuildRankRepository>()
                                                                 .Refresh(obj => obj.Id == currentRankId,
                                                                          obj => obj.Order = order);
                                                        order++;
                                                    }

                                                    DialogContext.SetValue("RankId", rank.Id);

                                                    bool repeat;

                                                    do
                                                    {
                                                        repeat = await RunSubElement<GuildRankEditDialogElement, bool>().ConfigureAwait(false);
                                                    }
                                                    while (repeat);
                                                }
                                            }

                                            return true;
                                        }
                             }
                         ];

            if (GetRanks().Count > 0)
            {
                _reactions.Add(new ReactionData<bool>
                               {
                                   Emote = DiscordEmoteService.GetEditEmote(CommandContext.Client),
                                   CommandText = LocalizationGroup.GetFormattedText("EditCommand", "{0} Edit rank", DiscordEmoteService.GetEditEmote(CommandContext.Client)),
                                   Func = async () =>
                                          {
                                              var levelId = await RunSubElement<GuildRankSelectionDialogElement, int>().ConfigureAwait(false);

                                              DialogContext.SetValue("RankId", levelId);

                                              bool repeat;

                                              do
                                              {
                                                  repeat = await RunSubElement<GuildRankEditDialogElement, bool>().ConfigureAwait(false);
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
                                              var rankId = await RunSubElement<GuildRankSelectionDialogElement, int>().ConfigureAwait(false);

                                              DialogContext.SetValue("RankId", rankId);

                                              return await RunSubElement<GuildRankDeletionDialogElement, bool>().ConfigureAwait(false);
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
    protected override bool DefaultFunc() => false;

    #endregion // DialogReactionElementBase<bool>
}