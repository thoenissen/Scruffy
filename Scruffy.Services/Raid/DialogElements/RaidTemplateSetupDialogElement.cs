using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.Raid.DialogElements.Forms;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// Starting the raid template assistant
/// </summary>
public class RaidTemplateSetupDialogElement : DialogEmbedReactionElementBase<bool>
{
    #region Fields

    /// <summary>
    /// Reactions
    /// </summary>
    private List<ReactionData<bool>> _reactions;

    /// <summary>
    /// Templates
    /// </summary>
    private List<string> _templates;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public RaidTemplateSetupDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Returns the existing templates
    /// </summary>
    /// <returns>Templates</returns>
    private List<string> GetTemplates()
    {
        if (_templates == null)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                _templates = dbFactory.GetRepository<RaidDayTemplateRepository>()
                                      .GetQuery()
                                      .Where(obj => obj.IsDeleted == false)
                                      .Select(obj => obj.AliasName)
                                      .OrderBy(obj => obj)
                                      .ToList();
            }
        }

        return _templates;
    }

    #endregion // Methods

    #region DialogReactionElementBase<bool>

    /// <inheritdoc/>
    public override Task EditMessage(EmbedBuilder builder)
    {
        builder.WithTitle(LocalizationGroup.GetText("ChooseCommandTitle", "Raid template configuration"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseCommandDescription", "With this assistant you are able to configure the raid templates. The following templates are already created:"));

        var templatesBuilder = new StringBuilder();

        var templates = GetTemplates();

        if (templates.Count > 0)
        {
            foreach (var template in templates)
            {
                templatesBuilder.AppendLine(Format.Bold($"{DiscordEmoteService.GetBulletEmote(CommandContext.Client)} {template}"));
            }
        }
        else
        {
            templatesBuilder.Append('\u200B');
        }

        builder.AddField(LocalizationGroup.GetText("TemplatesField", "Templates"), templatesBuilder.ToString());

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
                                 CommandText = LocalizationGroup.GetFormattedText("AddCommand", "{0} Add template", DiscordEmoteService.GetAddEmote(CommandContext.Client)),
                                 Func = async () =>
                                        {
                                            var data = await DialogHandler.RunForm<CreateRaidTemplateFormData>(CommandContext, false)
                                                                          .ConfigureAwait(false);

                                            using (var dbFactory = RepositoryFactory.CreateInstance())
                                            {
                                                dbFactory.GetRepository<RaidDayTemplateRepository>()
                                                         .Add(new RaidDayTemplateEntity
                                                              {
                                                                  AliasName = data.AliasName,
                                                                  Title = data.Title,
                                                                  Description = data.Description,
                                                                  Thumbnail = data.Thumbnail
                                                              });
                                            }

                                            return true;
                                        }
                             }
                         };

            if (GetTemplates().Count > 0)
            {
                _reactions.Add(new ReactionData<bool>
                               {
                                   Emote = DiscordEmoteService.GetEditEmote(CommandContext.Client),
                                   CommandText = LocalizationGroup.GetFormattedText("EditCommand", "{0} Edit template", DiscordEmoteService.GetEditEmote(CommandContext.Client)),
                                   Func = async () =>
                                          {
                                              var templateId = await RunSubElement<RaidTemplateSelectionDialogElement, long>().ConfigureAwait(false);

                                              DialogContext.SetValue("TemplateId", templateId);

                                              bool repeat;

                                              do
                                              {
                                                  repeat = await RunSubElement<RaidTemplateEditDialogElement, bool>().ConfigureAwait(false);
                                              }
                                              while (repeat);

                                              return true;
                                          }
                               });

                _reactions.Add(new ReactionData<bool>
                               {
                                   Emote = DiscordEmoteService.GetTrashCanEmote(CommandContext.Client),
                                   CommandText = LocalizationGroup.GetFormattedText("DeleteCommand", "{0} Delete template", DiscordEmoteService.GetTrashCanEmote(CommandContext.Client)),
                                   Func = async () =>
                                          {
                                              var templateId = await RunSubElement<RaidTemplateSelectionDialogElement, long>().ConfigureAwait(false);

                                              DialogContext.SetValue("TemplateId", templateId);

                                              return await RunSubElement<RaidTemplateDeletionElementBase, bool>().ConfigureAwait(false);
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