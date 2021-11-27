using DSharpPlus;
using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Tables.Guild;
using Scruffy.Data.Services.GuildAdministration;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.GuildAdministration.DialogElements.Forms;

namespace Scruffy.Services.GuildAdministration.DialogElements;

/// <summary>
/// Guild rank setup
/// </summary>
public class GuildAdministrationRankSetupDialogElement : DialogEmbedReactionElementBase<bool>
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
    public GuildAdministrationRankSetupDialogElement(LocalizationService localizationService)
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
                _ranks = new List<GuildRankData>();

                var ranks = dbFactory.GetRepository<GuildRankRepository>()
                                     .GetQuery()
                                     .Where(obj => obj.Guild.DiscordServerId == CommandContext.Guild.Id)
                                     .Select(obj => new
                                                    {
                                                        obj.SuperiorId,
                                                        obj.Id,
                                                        obj.InGameName,
                                                        obj.DiscordRoleId
                                                    })
                                     .ToList();

                var rank = ranks.FirstOrDefault(obj => obj.SuperiorId == null);
                while (rank != null)
                {
                    _ranks.Add(new GuildRankData
                               {
                                   Id = rank.Id,
                                   InGameName = rank.InGameName,
                                   DiscordRoleId = rank.DiscordRoleId
                               });

                    rank = ranks.FirstOrDefault(obj => obj.SuperiorId == rank.Id);
                }
            }
        }

        return _ranks;
    }

    #endregion // Methods

    #region DialogReactionElementBase<bool>

    /// <summary>
    /// Editing the embedded message
    /// </summary>
    /// <param name="builder">Builder</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override Task EditMessage(DiscordEmbedBuilder builder)
    {
        builder.WithTitle(LocalizationGroup.GetText("ChooseCommandTitle", "Guild special rank configuration"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseCommandDescription", "With this assistant you are able to configure the guild special ranks. The following special ranks are already created:"));

        var levelsBuilder = new StringBuilder();

        var ranks = GetRanks();
        if (ranks.Count > 0)
        {
            foreach (var rank in ranks)
            {
                levelsBuilder.AppendLine(Formatter.Bold($"{rank.InGameName} ({CommandContext.Guild.GetRole(rank.DiscordRoleId).Mention})"));
            }
        }
        else
        {
            levelsBuilder.Append('\u200B');
        }

        builder.AddField(LocalizationGroup.GetText("RanksFields", "Ranks"), levelsBuilder.ToString());

        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns the reactions which should be added to the message
    /// </summary>
    /// <returns>Reactions</returns>
    public override IReadOnlyList<ReactionData<bool>> GetReactions()
    {
        if (_reactions == null)
        {
            _reactions = new List<ReactionData<bool>>
                         {
                             new ()
                             {
                                 Emoji = DiscordEmojiService.GetAddEmoji(CommandContext.Client),
                                 CommandText = LocalizationGroup.GetFormattedText("AddCommand", "{0} Add rank", DiscordEmojiService.GetAddEmoji(CommandContext.Client)),
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
                                                               SuperiorId = data.SuperiorId
                                                           };

                                                if (dbFactory.GetRepository<GuildRankRepository>()
                                                             .Add(rank))
                                                {
                                                    DialogContext.SetValue("RankId", rank.Id);

                                                    bool repeat;

                                                    do
                                                    {
                                                        repeat = await RunSubElement<GuildAdministrationRankEditDialogElement, bool>().ConfigureAwait(false);
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
                                   Emoji = DiscordEmojiService.GetEditEmoji(CommandContext.Client),
                                   CommandText = LocalizationGroup.GetFormattedText("EditCommand", "{0} Edit rank", DiscordEmojiService.GetEditEmoji(CommandContext.Client)),
                                   Func = async () =>
                                          {
                                              var levelId = await RunSubElement<GuildAdministrationRankSelectionDialogElement, int>().ConfigureAwait(false);

                                              DialogContext.SetValue("RankId", levelId);

                                              bool repeat;

                                              do
                                              {
                                                  repeat = await RunSubElement<GuildAdministrationRankEditDialogElement, bool>().ConfigureAwait(false);
                                              }
                                              while (repeat);

                                              return true;
                                          }
                               });

                _reactions.Add(new ReactionData<bool>
                               {
                                   Emoji = DiscordEmojiService.GetTrashCanEmoji(CommandContext.Client),
                                   CommandText = LocalizationGroup.GetFormattedText("DeleteCommand", "{0} Delete rank", DiscordEmojiService.GetTrashCanEmoji(CommandContext.Client)),
                                   Func = async () =>
                                          {
                                              var rankId = await RunSubElement<GuildAdministrationRankSelectionDialogElement, int>().ConfigureAwait(false);

                                              DialogContext.SetValue("RankId", rankId);

                                              return await RunSubElement<GuildAdministrationRankDeletionDialogElement, bool>().ConfigureAwait(false);
                                          }
                               });
            }

            _reactions.Add(new ReactionData<bool>
                           {
                               Emoji = DiscordEmojiService.GetCrossEmoji(CommandContext.Client),
                               CommandText = LocalizationGroup.GetFormattedText("CancelCommand", "{0} Cancel", DiscordEmojiService.GetCrossEmoji(CommandContext.Client)),
                               Func = () => Task.FromResult(false)
                           });
        }

        return _reactions;
    }

    /// <summary>
    /// Returns the title of the commands
    /// </summary>
    /// <returns>Commands</returns>
    protected override string GetCommandTitle() => LocalizationGroup.GetText("CommandTitle", "Commands");

    /// <summary>
    /// Default case if none of the given reactions is used
    /// </summary>
    /// <returns>Result</returns>
    protected override bool DefaultFunc()
    {
        return false;
    }

    #endregion // DialogReactionElementBase<bool>
}