using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;

namespace Scruffy.Services.Raid.DialogElements
{
    /// <summary>
    /// Editing a raid experience level
    /// </summary>
    public class RaidExperienceLevelEditDialogElement : DialogEmbedReactionElementBase<bool>
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
        public RaidExperienceLevelEditDialogElement(LocalizationService localizationService)
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
            builder.WithTitle(LocalizationGroup.GetText("ChooseCommandTitle", "Raid experience level configuration"));
            builder.WithDescription(LocalizationGroup.GetText("ChooseCommandDescription", "With this assistant you are able to configure the raid experience level."));

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var templateId = DialogContext.GetValue<long>("ExperienceLevelId");

                var data = dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                    .GetQuery()
                                    .Where(obj => obj.Id == templateId)
                                    .Select(obj => new
                                    {
                                        obj.Description,
                                        obj.DiscordEmoji,
                                        obj.DiscordRoleId
                                    })
                                    .First();

                builder.AddField(LocalizationGroup.GetText("Description", "Description"), data.Description);
                builder.AddField(LocalizationGroup.GetText("Emoji", "Emoji"), DiscordEmoji.FromGuildEmote(CommandContext.Client, data.DiscordEmoji));

                if (data.DiscordRoleId != null)
                {
                    builder.AddField(LocalizationGroup.GetText("Role", "Role"), CommandContext.Guild.Roles[data.DiscordRoleId.Value].Mention);
                }
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
                                          CommandText = LocalizationGroup.GetFormattedText("EditSuperiorRoleCommand", "{0} Edit superior role", DiscordEmojiService.GetEditEmoji(CommandContext.Client)),
                                          Func = async () =>
                                                 {
                                                     var newSuperiorLevelId = await RunSubElement<RaidExperienceLevelSuperiorLevelDialogElement, long?>()
                                                                           .ConfigureAwait(false);

                                                     using (var dbFactory = RepositoryFactory.CreateInstance())
                                                     {
                                                         var levelId = DialogContext.GetValue<long>("ExperienceLevelId");

                                                         dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                                                  .Refresh(obj => obj.SuperiorExperienceLevelId == levelId,
                                                                           obj => obj.SuperiorExperienceLevelId = obj.SuperiorRaidExperienceLevel.SuperiorExperienceLevelId);

                                                         if (dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                                                      .RefreshRange(obj =>  obj.Id != levelId,
                                                                                    obj => obj.SuperiorExperienceLevelId = newSuperiorLevelId))
                                                         {
                                                             dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                                                      .RefreshRange(obj =>  obj.SuperiorExperienceLevelId == newSuperiorLevelId
                                                                                         && obj.Id != levelId,
                                                                                    obj => obj.SuperiorExperienceLevelId = levelId);
                                                         }
                                                     }

                                                     return true;
                                                 }
                                      },
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetEdit2Emoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("EditDescriptionCommand", "{0} Edit description", DiscordEmojiService.GetEdit2Emoji(CommandContext.Client)),
                                          Func = async () =>
                                                 {
                                                     var description = await RunSubElement<RaidExperienceLevelDescriptionDialogElement, string>()
                                                                           .ConfigureAwait(false);

                                                     using (var dbFactory = RepositoryFactory.CreateInstance())
                                                     {
                                                         var templateId = DialogContext.GetValue<long>("ExperienceLevelId");

                                                         dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                                                  .Refresh(obj => obj.Id == templateId, obj => obj.Description = description);
                                                     }

                                                     return true;
                                                 }
                                      },
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetEdit3Emoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("EditRoleCommand", "{0} Edit role", DiscordEmojiService.GetEdit3Emoji(CommandContext.Client)),
                                          Func = async () =>
                                                 {
                                                     var role = await RunSubElement<RaidExperienceLevelRoleDialogElement, ulong?>()
                                                                           .ConfigureAwait(false);

                                                     using (var dbFactory = RepositoryFactory.CreateInstance())
                                                     {
                                                         var templateId = DialogContext.GetValue<long>("ExperienceLevelId");

                                                         dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                                                  .Refresh(obj => obj.Id == templateId, obj => obj.DiscordRoleId = role);
                                                     }

                                                     return true;
                                                 }
                                      },
                                      new ReactionData<bool>
                                      {
                                          Emoji = DiscordEmojiService.GetEmojiEmoji(CommandContext.Client),
                                          CommandText = LocalizationGroup.GetFormattedText("EditEmojiCommand", "{0} Edit emoji", DiscordEmojiService.GetEmojiEmoji(CommandContext.Client)),
                                          Func = async () =>
                                                 {
                                                     var emoji = await RunSubElement<RaidExperienceLevelEmojiDialogElement, ulong>()
                                                                     .ConfigureAwait(false);

                                                     using (var dbFactory = RepositoryFactory.CreateInstance())
                                                     {
                                                         var templateId = DialogContext.GetValue<long>("ExperienceLevelId");

                                                         dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                                                  .Refresh(obj => obj.Id == templateId, obj => obj.DiscordEmoji = emoji);
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