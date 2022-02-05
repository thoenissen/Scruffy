using Discord;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid.DialogElements;

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
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task EditMessage(EmbedBuilder builder)
    {
        builder.WithTitle(LocalizationGroup.GetText("ChooseCommandTitle", "Raid experience level configuration"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseCommandDescription", "With this assistant you are able to configure the raid experience level."));

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var templateId = DialogContext.GetValue<long>("ExperienceLevelId");

            var data = await dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                      .GetQuery()
                                      .Where(obj => obj.Id == templateId)
                                      .Select(obj => new
                                                     {
                                                         obj.Description,
                                                         obj.AliasName,
                                                         obj.DiscordEmoji,
                                                         obj.DiscordRoleId
                                                     })
                                      .FirstAsync()
                                      .ConfigureAwait(false);

            builder.AddField(LocalizationGroup.GetText("Description", "Description"), data.Description);
            builder.AddField(LocalizationGroup.GetText("AliasName", "Alias name"), data.AliasName);
            builder.AddField(LocalizationGroup.GetText("Emoji", "Emoji"), DiscordEmoteService.GetGuildEmote(CommandContext.Client, data.DiscordEmoji));

            if (data.DiscordRoleId != null)
            {
                builder.AddField(LocalizationGroup.GetText("Role", "Role"), CommandContext.Guild.Roles.FirstOrDefault(obj => obj.Id == data.DiscordRoleId)?.Mention);
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
                                  new ()
                                  {
                                      Emote = DiscordEmoteService.GetEditEmote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditSuperiorRoleCommand", "{0} Edit superior role", DiscordEmoteService.GetEditEmote(CommandContext.Client)),
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
                                                         if (dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                                                      .RefreshRange(obj =>  obj.SuperiorExperienceLevelId == newSuperiorLevelId
                                                                                         && obj.Id != levelId,
                                                                                    obj => obj.SuperiorExperienceLevelId = levelId))
                                                         {
                                                             dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                                                      .RefreshRanks();
                                                         }
                                                     }
                                                 }

                                                 return true;
                                             }
                                  },
                                  new ()
                                  {
                                      Emote = DiscordEmoteService.GetEdit2Emote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditDescriptionCommand", "{0} Edit description", DiscordEmoteService.GetEdit2Emote(CommandContext.Client)),
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
                                  new ()
                                  {
                                      Emote = DiscordEmoteService.GetEdit3Emote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditAliasNameCommand", "{0} Edit alias name", DiscordEmoteService.GetEdit3Emote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var aliasName = await RunSubElement<RaidExperienceLevelAliasNameDialogElement, string>()
                                                                     .ConfigureAwait(false);

                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var templateId = DialogContext.GetValue<long>("ExperienceLevelId");

                                                     dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                                              .Refresh(obj => obj.Id == templateId, obj => obj.AliasName = aliasName);
                                                 }

                                                 return true;
                                             }
                                  },
                                  new ()
                                  {
                                      Emote = DiscordEmoteService.GetEdit4Emote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditRoleCommand", "{0} Edit role", DiscordEmoteService.GetEdit4Emote(CommandContext.Client)),
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
                                  new ()
                                  {
                                      Emote = DiscordEmoteService.GetEmojiEmote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditEmojiCommand", "{0} Edit emoji", DiscordEmoteService.GetEmojiEmote(CommandContext.Client)),
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
                                  new ()
                                  {
                                      Emote = DiscordEmoteService.GetCrossEmote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("CancelCommand", "{0} Cancel", DiscordEmoteService.GetCrossEmote(CommandContext.Client)),
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