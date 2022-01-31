using Discord;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.Guild.DialogElements.Forms;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Editing a special rank
/// </summary>
public class GuildSpecialRankEditDialogElement : DialogEmbedReactionElementBase<bool>
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
    public GuildSpecialRankEditDialogElement(LocalizationService localizationService)
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
        builder.WithTitle(LocalizationGroup.GetText("ChooseCommandTitle", "Special rank configuration"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseCommandDescription", "With this assistant you are able to configure the special rank."));

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var rankId = DialogContext.GetValue<long>("RankId");

            var data = await  dbFactory.GetRepository<GuildSpecialRankConfigurationRepository>()
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
                                                                                     }),
                                                          IgnoreRoles = obj.GuildSpecialRankIgnoreRoleAssignments
                                                                           .Select(obj => obj.DiscordRoleId)
                                                      })
                                       .FirstAsync()
                                       .ConfigureAwait(false);

            var fieldBuilder = new StringBuilder();
            fieldBuilder.AppendLine($"{Format.Bold(LocalizationGroup.GetText("Description", "Description"))}: {data.Description}");
            fieldBuilder.AppendLine($"{Format.Bold(LocalizationGroup.GetText("DiscordRole", "Discord role"))}: {CommandContext.Guild.GetRole(data.DiscordRoleId).Mention}");
            fieldBuilder.AppendLine($"{Format.Bold(LocalizationGroup.GetText("MaximumPoints", "Maximum points"))}: {data.MaximumPoints.ToString(LocalizationGroup.CultureInfo)}");
            fieldBuilder.AppendLine($"{Format.Bold(LocalizationGroup.GetText("GrantThreshold", "Grant threshold"))}: {data.GrantThreshold.ToString(LocalizationGroup.CultureInfo)}");
            fieldBuilder.AppendLine($"{Format.Bold(LocalizationGroup.GetText("RemoveThreshold", "Remove threshold"))}: {data.RemoveThreshold.ToString(LocalizationGroup.CultureInfo)}");
            builder.AddField(LocalizationGroup.GetText("General", "General"), fieldBuilder.ToString());

            fieldBuilder.Clear();

            foreach (var role in data.Roles)
            {
                fieldBuilder.AppendLine($"{CommandContext.Guild.GetRole(role.DiscordRoleId).Mention}: {role.Points.ToString(LocalizationGroup.CultureInfo)}");
            }

            fieldBuilder.Append("\u200B");
            builder.AddField(LocalizationGroup.GetText("Roles", "Point roles"), fieldBuilder.ToString());

            fieldBuilder.Clear();

            foreach (var role in data.IgnoreRoles)
            {
                fieldBuilder.AppendLine(CommandContext.Guild.GetRole(role).Mention);
            }

            fieldBuilder.Append("\u200B");
            builder.AddField(LocalizationGroup.GetText("IgnoreRoles", "Ignore roles"), fieldBuilder.ToString());
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
                                      CommandText = LocalizationGroup.GetFormattedText("EditDescriptionCommand", "{0} Edit description", DiscordEmoteService.GetEditEmote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var description = await RunSubElement<GuildSpecialRankDescriptionDialogElement, string>().ConfigureAwait(false);

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
                                  new ()
                                  {
                                      Emote = DiscordEmoteService.GetEdit2Emote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditRoleCommand", "{0} Edit role", DiscordEmoteService.GetEdit2Emote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var roleId = await RunSubElement<GuildSpecialRankDiscordRoleDialogElement, ulong>().ConfigureAwait(false);

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
                                  new ()
                                  {
                                      Emote = DiscordEmoteService.GetEdit3Emote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditMaximumPointsCommand", "{0} Edit maximum points", DiscordEmoteService.GetEdit3Emote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var maximumPoints = await RunSubElement<GuildSpecialRankMaximumPointsDialogElement, double>().ConfigureAwait(false);

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
                                  new ()
                                  {
                                      Emote = DiscordEmoteService.GetEdit4Emote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditGrantThresholdCommand", "{0} Edit grant threshold", DiscordEmoteService.GetEdit4Emote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var grantThreshold = await RunSubElement<GuildSpecialRankGrantThresholdDialogElement, double>().ConfigureAwait(false);

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
                                  new ()
                                  {
                                      Emote = DiscordEmoteService.GetEdit5Emote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditRemoveThresholdCommand", "{0} Edit remove threshold", DiscordEmoteService.GetEdit5Emote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var removeThreshold = await RunSubElement<GuildSpecialRankRemoveThresholdDialogElement, double>().ConfigureAwait(false);

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
                                  new ()
                                  {
                                      Emote = DiscordEmoteService.GetAddEmote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("AddPointRoleCommand", "{0} Add point role", DiscordEmoteService.GetAddEmote(CommandContext.Client)),
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
                                  new ()
                                  {
                                      Emote = DiscordEmoteService.GetTrashCanEmote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("RemovePointRoleCommand", "{0} Remove point role", DiscordEmoteService.GetTrashCanEmote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var roleId = await RunSubElement<GuildSpecialRankRoleAssignmentSelectionDialog, ulong>().ConfigureAwait(false);

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
                                  new ()
                                  {
                                      Emote = DiscordEmoteService.GetAdd2Emote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("AddIgnoreRoleCommand", "{0} Add ignore role", DiscordEmoteService.GetAdd2Emote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var discordId = await RunSubElement<GuildSpecialRankRoleAssignmentDiscordRoleDialogElement, ulong>().ConfigureAwait(false);

                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var rankId = DialogContext.GetValue<long>("RankId");

                                                     dbFactory.GetRepository<GuildSpecialRankIgnoreRoleAssignmentRepository>()
                                                              .AddOrRefresh(obj => obj.ConfigurationId == rankId
                                                                                && obj.DiscordRoleId == discordId,
                                                                            obj =>
                                                                            {
                                                                                obj.ConfigurationId = rankId;
                                                                                obj.DiscordRoleId = discordId;
                                                                            });
                                                 }

                                                 return true;
                                             }
                                  },
                                  new ()
                                  {
                                      Emote = DiscordEmoteService.GetTrashCan2Emote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("RemoveIgnoreRoleCommand", "{0} Remove ignore role", DiscordEmoteService.GetTrashCan2Emote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var discordId = await RunSubElement<GuildSpecialRankRoleAssignmentDiscordRoleDialogElement, ulong>().ConfigureAwait(false);

                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var rankId = DialogContext.GetValue<long>("RankId");

                                                     dbFactory.GetRepository<GuildSpecialRankRoleAssignmentRepository>()
                                                              .Remove(obj => obj.ConfigurationId == rankId
                                                                          && obj.DiscordRoleId == discordId);
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