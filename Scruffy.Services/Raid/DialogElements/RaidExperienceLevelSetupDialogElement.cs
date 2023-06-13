using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Data.Services.Raid;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.Raid.DialogElements.Forms;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// Starting the raid experience level assistant
/// </summary>
public class RaidExperienceLevelSetupDialogElement : DialogEmbedReactionElementBase<bool>
{
    #region Fields

    /// <summary>
    /// Reactions
    /// </summary>
    private List<ReactionData<bool>> _reactions;

    /// <summary>
    /// Experience levels
    /// </summary>
    private List<RaidExperienceLevelData> _levels;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public RaidExperienceLevelSetupDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Returns the existing levels
    /// </summary>
    /// <returns>Levels</returns>
    private List<RaidExperienceLevelData> GetLevels()
    {
        if (_levels == null)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                _levels = dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                   .GetQuery()
                                   .Where(obj => obj.IsDeleted == false)
                                   .Select(obj => new RaidExperienceLevelData
                                                  {
                                                      Id = obj.Id,
                                                      SuperiorExperienceLevelId = obj.SuperiorExperienceLevelId,
                                                      Description = obj.Description,
                                                      DiscordEmoji = obj.DiscordEmoji,
                                                  })
                                   .ToList();
            }
        }

        return _levels;
    }

    #endregion // Methods

    #region DialogReactionElementBase<bool>

    /// <summary>
    /// Editing the embedded message
    /// </summary>
    /// <param name="builder">Builder</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override Task EditMessage(EmbedBuilder builder)
    {
        builder.WithTitle(LocalizationGroup.GetText("ChooseCommandTitle", "Raid experience levels configuration"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseCommandDescription", "With this assistant you are able to configure the raid experience levels. The following experience levels are already created:"));

        var levelsBuilder = new StringBuilder();

        var levels = GetLevels();
        if (levels.Count > 0)
        {
            var currentLevel = levels.FirstOrDefault(obj => obj.SuperiorExperienceLevelId == null);
            while (currentLevel != null)
            {
                levelsBuilder.AppendLine(Format.Bold($"{DiscordEmoteService.GetGuildEmote(CommandContext.Client, currentLevel.DiscordEmoji)} {currentLevel.Description}"));

                currentLevel = levels.FirstOrDefault(obj => obj.SuperiorExperienceLevelId == currentLevel.Id);
            }
        }
        else
        {
            levelsBuilder.Append('\u200B');
        }

        builder.AddField(LocalizationGroup.GetText("LevelsFields", "Levels"), levelsBuilder.ToString());

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
                             new()
                             {
                                 Emote = DiscordEmoteService.GetAddEmote(CommandContext.Client),
                                 CommandText = LocalizationGroup.GetFormattedText("AddCommand", "{0} Add level", DiscordEmoteService.GetAddEmote(CommandContext.Client)),
                                 Func = async () =>
                                        {
                                            var data = await DialogHandler.RunForm<CreateRaidExperienceLevelData>(CommandContext, false)
                                                                          .ConfigureAwait(false);

                                            using (var dbFactory = RepositoryFactory.CreateInstance())
                                            {
                                                var level = new RaidExperienceLevelEntity
                                                            {
                                                                SuperiorExperienceLevelId = data.SuperiorExperienceLevelId,
                                                                Description = data.Description,
                                                                DiscordEmoji = data.DiscordEmoji,
                                                                DiscordRoleId = data.DiscordRoleId,
                                                            };

                                                if (dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                                             .Add(level))
                                                {
                                                    if (dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                                                 .RefreshRange(obj => obj.SuperiorExperienceLevelId == data.SuperiorExperienceLevelId
                                                                                   && obj.Id != level.Id,
                                                                               obj => obj.SuperiorExperienceLevelId = level.Id))
                                                    {
                                                        dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                                                 .RefreshRanks();
                                                    }
                                                }
                                            }

                                            return true;
                                        }
                             }
                         };

            if (GetLevels().Count > 0)
            {
                _reactions.Add(new ReactionData<bool>
                               {
                                   Emote = DiscordEmoteService.GetEditEmote(CommandContext.Client),
                                   CommandText = LocalizationGroup.GetFormattedText("EditCommand", "{0} Edit level", DiscordEmoteService.GetEditEmote(CommandContext.Client)),
                                   Func = async () =>
                                          {
                                              var levelId = await RunSubElement<RaidExperienceLevelSelectionDialogElement, long>().ConfigureAwait(false);

                                              DialogContext.SetValue("ExperienceLevelId", levelId);

                                              bool repeat;

                                              do
                                              {
                                                  repeat = await RunSubElement<RaidExperienceLevelEditDialogElement, bool>().ConfigureAwait(false);
                                              }
                                              while (repeat);

                                              return true;
                                          }
                               });

                _reactions.Add(new ReactionData<bool>
                               {
                                   Emote = DiscordEmoteService.GetTrashCanEmote(CommandContext.Client),
                                   CommandText = LocalizationGroup.GetFormattedText("DeleteCommand", "{0} Delete level", DiscordEmoteService.GetTrashCanEmote(CommandContext.Client)),
                                   Func = async () =>
                                          {
                                              var levelId = await RunSubElement<RaidExperienceLevelSelectionDialogElement, long>().ConfigureAwait(false);

                                              DialogContext.SetValue("ExperienceLevelId", levelId);

                                              return await RunSubElement<RaidExperienceLevelDeletionDialogElement, bool>().ConfigureAwait(false);
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