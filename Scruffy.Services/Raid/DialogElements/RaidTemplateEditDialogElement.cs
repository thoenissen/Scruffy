using Discord;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// Editing a raid template
/// </summary>
public class RaidTemplateEditDialogElement : DialogEmbedReactionElementBase<bool>
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
    public RaidTemplateEditDialogElement(LocalizationService localizationService)
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
        builder.WithTitle(LocalizationGroup.GetText("ChooseCommandTitle", "Raid template configuration"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseCommandDescription", "With this assistant you are able to configure the raid template."));

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var templateId = DialogContext.GetValue<long>("TemplateId");

            var data = await dbFactory.GetRepository<RaidDayTemplateRepository>()
                                      .GetQuery()
                                      .Where(obj => obj.Id == templateId)
                                      .Select(obj => new
                                                     {
                                                         obj.AliasName,
                                                         obj.Title,
                                                         obj.Description,
                                                         obj.Thumbnail
                                                     })
                                      .FirstAsync()
                                      .ConfigureAwait(false);

            builder.AddField(LocalizationGroup.GetText("AliasName", "Alias name"), data.AliasName);
            builder.AddField(LocalizationGroup.GetText("Title", "Title"), data.Title);
            builder.AddField(LocalizationGroup.GetText("Description", "Description"), data.Description);
            builder.WithThumbnailUrl(data.Thumbnail);
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
                                  new()
                                  {
                                      Emote = DiscordEmoteService.GetEditEmote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditAliasCommand", "{0} Edit alias name", DiscordEmoteService.GetEditEmote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var aliasName = await RunSubElement<RaidTemplateAliasNameDialogElement, string>()
                                                                     .ConfigureAwait(false);

                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var templateId = DialogContext.GetValue<long>("TemplateId");

                                                     dbFactory.GetRepository<RaidDayTemplateRepository>()
                                                              .Refresh(obj => obj.Id == templateId, obj => obj.AliasName = aliasName);
                                                 }

                                                 return true;
                                             }
                                  },
                                  new()
                                  {
                                      Emote = DiscordEmoteService.GetEdit2Emote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditTitleCommand", "{0} Edit title", DiscordEmoteService.GetEdit2Emote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var title = await RunSubElement<RaidTemplateTitleDialogElement, string>()
                                                                 .ConfigureAwait(false);

                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var templateId = DialogContext.GetValue<long>("TemplateId");

                                                     dbFactory.GetRepository<RaidDayTemplateRepository>()
                                                              .Refresh(obj => obj.Id == templateId, obj => obj.Title = title);
                                                 }

                                                 return true;
                                             }
                                  },
                                  new()
                                  {
                                      Emote = DiscordEmoteService.GetEdit3Emote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditDescriptionCommand", "{0} Edit description", DiscordEmoteService.GetEdit3Emote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var description = await RunSubElement<RaidTemplateDescriptionDialogElement, string>()
                                                                       .ConfigureAwait(false);

                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var templateId = DialogContext.GetValue<long>("TemplateId");

                                                     dbFactory.GetRepository<RaidDayTemplateRepository>()
                                                              .Refresh(obj => obj.Id == templateId, obj => obj.Description = description);
                                                 }

                                                 return true;
                                             }
                                  },
                                  new()
                                  {
                                      Emote = DiscordEmoteService.GetImageEmote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditThumbnailCommand", "{0} Edit thumbnail", DiscordEmoteService.GetImageEmote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var thumbnail = await RunSubElement<RaidTemplateThumbnailDialogElement, string>()
                                                                     .ConfigureAwait(false);

                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var templateId = DialogContext.GetValue<long>("TemplateId");

                                                     dbFactory.GetRepository<RaidDayTemplateRepository>()
                                                              .Refresh(obj => obj.Id == templateId, obj => obj.Thumbnail = thumbnail);
                                                 }

                                                 return true;
                                             }
                                  },
                                  new()
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