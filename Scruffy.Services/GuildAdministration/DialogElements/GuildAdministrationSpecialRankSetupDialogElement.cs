using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildAdministration;
using Scruffy.Data.Entity.Tables.GuildAdministration;
using Scruffy.Data.Services.GuildAdministration;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.GuildAdministration.DialogElements.Forms;

namespace Scruffy.Services.GuildAdministration.DialogElements
{
    /// <summary>
    /// Starting the guild special ranks assistant
    /// </summary>
    public class GuildAdministrationSpecialRankSetupDialogElement : DialogEmbedReactionElementBase<bool>
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
        public GuildAdministrationSpecialRankSetupDialogElement(LocalizationService localizationService)
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
                    var role = CommandContext.Guild.GetRole(rank.DiscordRoleId);

                    levelsBuilder.AppendLine(Formatter.Bold($"{rank.Description} ({role.Mention})"));
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
                                 new ReactionData<bool>
                                 {
                                     Emoji = DiscordEmojiService.GetAddEmoji(CommandContext.Client),
                                     CommandText = LocalizationGroup.GetFormattedText("AddCommand", "{0} Add rank", DiscordEmojiService.GetAddEmoji(CommandContext.Client)),
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
                                                            repeat = await RunSubElement<GuildAdministrationSpecialRankEditDialogElement, bool>().ConfigureAwait(false);
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
                            var levelId = await RunSubElement<GuildAdministrationSpecialRankSelectionDialogElement, long>().ConfigureAwait(false);

                            DialogContext.SetValue("RankId", levelId);

                            bool repeat;

                            do
                            {
                                repeat = await RunSubElement<GuildAdministrationSpecialRankEditDialogElement, bool>().ConfigureAwait(false);
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
                            var rankId = await RunSubElement<GuildAdministrationSpecialRankSelectionDialogElement, long>().ConfigureAwait(false);

                            DialogContext.SetValue("RankId", rankId);

                            return await RunSubElement<GuildAdministrationSpecialRankDeletionDialogElement, bool>().ConfigureAwait(false);
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
}
