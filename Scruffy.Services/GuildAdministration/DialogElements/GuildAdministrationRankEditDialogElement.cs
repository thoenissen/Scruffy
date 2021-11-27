using DSharpPlus;
using DSharpPlus.Entities;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.GuildAdministration.DialogElements;

/// <summary>
/// Editing a special rank
/// </summary>
public class GuildAdministrationRankEditDialogElement : DialogEmbedReactionElementBase<bool>
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
    public GuildAdministrationRankEditDialogElement(LocalizationService localizationService)
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
    public override async Task EditMessage(DiscordEmbedBuilder builder)
    {
        builder.WithTitle(LocalizationGroup.GetText("ChooseCommandTitle", "Rank configuration"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseCommandDescription", "With this assistant you are able to configure the rank."));

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var rankId = DialogContext.GetValue<int>("RankId");

            var data = await dbFactory.GetRepository<GuildRankRepository>()
                                       .GetQuery()
                                       .Where(obj => obj.Id == rankId)
                                       .Select(obj => new
                                       {
                                           obj.InGameName,
                                           obj.DiscordRoleId,
                                           obj.Percentage
                                       })
                                       .FirstAsync()
                                       .ConfigureAwait(false);

            var fieldBuilder = new StringBuilder();
            fieldBuilder.AppendLine($"{Formatter.Bold(LocalizationGroup.GetText("InGameName", "In game name"))}: {data.InGameName}");
            fieldBuilder.AppendLine($"{Formatter.Bold(LocalizationGroup.GetText("DiscordRoleId", "Discord role"))}: {CommandContext.Guild.GetRole(data.DiscordRoleId).Mention}");
            fieldBuilder.AppendLine($"{Formatter.Bold(LocalizationGroup.GetText("Percentage", "Percentage quota"))}: {data.Percentage.ToString(LocalizationGroup.CultureInfo)}");
            builder.AddField(LocalizationGroup.GetText("General", "General"), fieldBuilder.ToString());
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
                                      Emoji = DiscordEmojiService.GetEditEmoji(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditInGameNameCommand", "{0} Edit in game name", DiscordEmojiService.GetEditEmoji(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var inGameName = await RunSubElement<GuildAdministrationRankInGameNameDialogElement, string>().ConfigureAwait(false);

                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var rankId = DialogContext.GetValue<int>("RankId");

                                                     dbFactory.GetRepository<GuildRankRepository>()
                                                              .Refresh(obj => obj.Id == rankId,
                                                                       obj => obj.InGameName = inGameName);
                                                 }

                                                 return true;
                                             }
                                  },
                                  new ()
                                  {
                                      Emoji = DiscordEmojiService.GetEdit2Emoji(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditDiscordCommand", "{0} Edit discord role", DiscordEmojiService.GetEdit2Emoji(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var roleId = await RunSubElement<GuildAdministrationRankDiscordRoleDialogElement, ulong>().ConfigureAwait(false);

                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var rankId = DialogContext.GetValue<int>("RankId");

                                                     dbFactory.GetRepository<GuildRankRepository>()
                                                              .Refresh(obj => obj.Id == rankId,
                                                                       obj => obj.DiscordRoleId = roleId);
                                                 }

                                                 return true;
                                             }
                                  },
                                  new ()
                                  {
                                      Emoji = DiscordEmojiService.GetEdit3Emoji(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditPercentageCommand", "{0} Edit percentage", DiscordEmojiService.GetEdit3Emoji(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var percentage = await RunSubElement<GuildAdministrationRankPercentageDialogElement, double>().ConfigureAwait(false);

                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var rankId = DialogContext.GetValue<int>("RankId");

                                                     dbFactory.GetRepository<GuildRankRepository>()
                                                              .Refresh(obj => obj.Id == rankId,
                                                                       obj => obj.Percentage = percentage);
                                                 }

                                                 return true;
                                             }
                                  },
                                  new ()
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