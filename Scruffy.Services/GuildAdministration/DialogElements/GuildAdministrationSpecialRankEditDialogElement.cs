using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildAdministration;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.GuildAdministration.DialogElements.Forms;

namespace Scruffy.Services.GuildAdministration.DialogElements
{
    /// <summary>
    /// Editing a special rank
    /// </summary>
    public class GuildAdministrationSpecialRankEditDialogElement : DialogEmbedReactionElementBase<bool>
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
        public GuildAdministrationSpecialRankEditDialogElement(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region DialogReactionElementBase<bool>

        /// <summary>
        /// Editing the embedded message
        /// </summary>
        /// <param name="builder">Builder</param>
        public override void EditMessage(DiscordEmbedBuilder builder)
        {
            builder.WithTitle(LocalizationGroup.GetText("ChooseCommandTitle", "Special rank configuration"));
            builder.WithDescription(LocalizationGroup.GetText("ChooseCommandDescription", "With this assistant you are able to configure the special rank."));

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var rankId = DialogContext.GetValue<long>("RankId");

                var data = dbFactory.GetRepository<GuildSpecialRankConfigurationRepository>()
                                    .GetQuery()
                                    .Where(obj => obj.Id == rankId)
                                    .Select(obj => new
                                    {
                                        obj.Description,
                                        obj.DiscordRoleId,
                                        obj.MaximumPoints,
                                        obj.GrantThreshold,
                                        obj.RemoveThreshold,
                                        Roles = obj.GuildSpecialRankRoleAssignments
                                                   .Select(obj2 => new
                                                                   {
                                                                       obj2.DiscordRoleId,
                                                                       obj2.Points
                                                                   })
                                    })
                                    .First();

                var fieldBuilder = new StringBuilder();
                fieldBuilder.AppendLine($"{Formatter.Bold(LocalizationGroup.GetText("Description", "Description"))}: {data.Description}");
                fieldBuilder.AppendLine($"{Formatter.Bold(LocalizationGroup.GetText("DiscordRole", "Discord role"))}: {CommandContext.Guild.GetRole(data.DiscordRoleId).Mention}");
                fieldBuilder.AppendLine($"{Formatter.Bold(LocalizationGroup.GetText("MaximumPoints", "Maximum points"))}: {data.MaximumPoints.ToString(LocalizationGroup.CultureInfo)}");
                fieldBuilder.AppendLine($"{Formatter.Bold(LocalizationGroup.GetText("GrantThreshold", "Grant threshold"))}: {data.GrantThreshold.ToString(LocalizationGroup.CultureInfo)}");
                fieldBuilder.AppendLine($"{Formatter.Bold(LocalizationGroup.GetText("RemoveThreshold", "Remove threshold"))}: {data.RemoveThreshold.ToString(LocalizationGroup.CultureInfo)}");
                builder.AddField(LocalizationGroup.GetText("General", "General"), fieldBuilder.ToString());

                fieldBuilder.Clear();

                foreach (var role in data.Roles)
                {
                    fieldBuilder.AppendLine($"{CommandContext.Guild.GetRole(role.DiscordRoleId).Mention}: {role.Points.ToString(LocalizationGroup.CultureInfo)}");
                }

                fieldBuilder.Append("\u200B");
                builder.AddField(LocalizationGroup.GetText("Roles", "Roles"), fieldBuilder.ToString());
            }
        }

        /// <summary>
        /// Returns the title of the commands
        /// </summary>
        /// <returns>Commands</returns>
        protected override string GetCommandTitle()
        {
            return LocalizationGroup.GetText("CommandTitle", "Commands");
        }

        /// <summary>
        /// Returns the reactions which should be added to the message
        /// </summary>
        /// <returns>Reactions</returns>
        public override IReadOnlyList<ReactionData<bool>> GetReactions()
        {
            return _reactions ??= new List<ReactionData<bool>>
                                  {
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetEditEmoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("EditDescriptionCommand", "{0} Edit description", DiscordEmojiService.GetEditEmoji(CommandContext.Client)),
                                          Func = async () =>
                                                 {
                                                     var description = await RunSubElement<GuildAdministrationSpecialRankDescriptionDialogElement, string>().ConfigureAwait(false);

                                                     using (var dbFactory = RepositoryFactory.CreateInstance())
                                                     {
                                                         var rankId = DialogContext.GetValue<long>("RankId");

                                                         dbFactory.GetRepository<GuildSpecialRankConfigurationRepository>()
                                                                  .Refresh(obj => obj.Id == rankId,
                                                                           obj => obj.Description = description);
                                                     }

                                                     return true;
                                                 }
                                      },
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetEdit2Emoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("EditRoleCommand", "{0} Edit role", DiscordEmojiService.GetEdit2Emoji(CommandContext.Client)),
                                          Func = async () =>
                                                 {
                                                     var roleId = await RunSubElement<GuildAdministrationSpecialRankDiscordRoleDialogElement, ulong>().ConfigureAwait(false);

                                                     using (var dbFactory = RepositoryFactory.CreateInstance())
                                                     {
                                                         var rankId = DialogContext.GetValue<long>("RankId");

                                                         dbFactory.GetRepository<GuildSpecialRankConfigurationRepository>()
                                                                  .Refresh(obj => obj.Id == rankId,
                                                                           obj => obj.DiscordRoleId = roleId);
                                                     }

                                                     return true;
                                                 }
                                      },
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetEdit3Emoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("EditMaximumPointsCommand", "{0} Edit maximum points", DiscordEmojiService.GetEdit3Emoji(CommandContext.Client)),
                                          Func = async () =>
                                                 {
                                                     var maximumPoints = await RunSubElement<GuildAdministrationSpecialRankMaximumPointsDialogElement, double>().ConfigureAwait(false);

                                                     using (var dbFactory = RepositoryFactory.CreateInstance())
                                                     {
                                                         var rankId = DialogContext.GetValue<long>("RankId");

                                                         dbFactory.GetRepository<GuildSpecialRankConfigurationRepository>()
                                                                  .Refresh(obj => obj.Id == rankId,
                                                                           obj => obj.MaximumPoints = maximumPoints);
                                                     }

                                                     return true;
                                                 }
                                      },
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetEdit4Emoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("EditGrantThresholdCommand", "{0} Edit grant threshold", DiscordEmojiService.GetEdit4Emoji(CommandContext.Client)),
                                          Func = async () =>
                                                 {
                                                     var grantThreshold = await RunSubElement<GuildAdministrationSpecialRankGrantThresholdDialogElement, double>().ConfigureAwait(false);

                                                     using (var dbFactory = RepositoryFactory.CreateInstance())
                                                     {
                                                         var rankId = DialogContext.GetValue<long>("RankId");

                                                         dbFactory.GetRepository<GuildSpecialRankConfigurationRepository>()
                                                                  .Refresh(obj => obj.Id == rankId,
                                                                           obj => obj.GrantThreshold = grantThreshold);
                                                     }

                                                     return true;
                                                 }
                                      },
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetEdit5Emoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("EditRemoveThresholdCommand", "{0} Edit remove threshold", DiscordEmojiService.GetEdit5Emoji(CommandContext.Client)),
                                          Func = async () =>
                                                 {
                                                     var removeThreshold = await RunSubElement<GuildAdministrationSpecialRankRemoveThresholdDialogElement, double>().ConfigureAwait(false);

                                                     using (var dbFactory = RepositoryFactory.CreateInstance())
                                                     {
                                                         var rankId = DialogContext.GetValue<long>("RankId");

                                                         dbFactory.GetRepository<GuildSpecialRankConfigurationRepository>()
                                                                  .Refresh(obj => obj.Id == rankId,
                                                                           obj => obj.RemoveThreshold = removeThreshold);
                                                     }

                                                     return true;
                                                 }
                                      },
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetAddEmoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("AddPointRoleCommand", "{0} Add point role", DiscordEmojiService.GetAddEmoji(CommandContext.Client)),
                                          Func = async () =>
                                                 {
                                                     var assignmentData = await RunSubForm<CreateGuildSpecialRankRoleAssignment>().ConfigureAwait(false);

                                                     using (var dbFactory = RepositoryFactory.CreateInstance())
                                                     {
                                                         var rankId = DialogContext.GetValue<long>("RankId");

                                                         dbFactory.GetRepository<GuildSpecialRankRoleAssignmentRepository>()
                                                                  .AddOrRefresh(obj => obj.ConfigurationId == rankId
                                                                               && obj.DiscordRoleId == assignmentData.DiscordRoleId,
                                                                           obj =>
                                                                           {
                                                                               obj.ConfigurationId = rankId;
                                                                               obj.DiscordRoleId = assignmentData.DiscordRoleId;
                                                                               obj.Points = assignmentData.Points;
                                                                           });
                                                     }

                                                     return true;
                                                 }
                                      },
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetTrashCanEmoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("RemovePointRoleCommand", "{0} Remove point role", DiscordEmojiService.GetTrashCanEmoji(CommandContext.Client)),
                                          Func = async () =>
                                                 {
                                                     var roleId = await RunSubElement<GuildAdministrationSpecialRankRoleAssignmentSelectionDialogElement, ulong>().ConfigureAwait(false);

                                                     using (var dbFactory = RepositoryFactory.CreateInstance())
                                                     {
                                                         var rankId = DialogContext.GetValue<long>("RankId");

                                                         dbFactory.GetRepository<GuildSpecialRankRoleAssignmentRepository>()
                                                                  .Remove(obj => obj.ConfigurationId == rankId
                                                                              && obj.DiscordRoleId == roleId);
                                                     }

                                                     return true;
                                                 }
                                      },
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetCrossEmoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("CancelCommand", "{0} Cancel", DiscordEmojiService.GetCrossEmoji(CommandContext.Client)),
                                          Func = () => Task.FromResult(false)
                                      }
                                  };
        }

        /// <summary>
        /// Default case if none of the given reactions is used
        /// </summary>
        /// <returns>Result</returns>
        protected override bool DefaultFunc()
        {
            return false;
        }

        #endregion DialogReactionElementBase<bool>
    }
}