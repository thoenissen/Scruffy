using Discord;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Editing a special rank
/// </summary>
public class GuildRankEditDialogElement : DialogEmbedReactionElementBase<bool>
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
    public GuildRankEditDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogReactionElementBase<bool>

    /// <inheritdoc/>
    public override async Task EditMessage(EmbedBuilder builder)
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
            fieldBuilder.AppendLine($"{Format.Bold(LocalizationGroup.GetText("InGameName", "In game name"))}: {data.InGameName}");
            fieldBuilder.AppendLine($"{Format.Bold(LocalizationGroup.GetText("DiscordRoleId", "Discord role"))}: {CommandContext.Guild.GetRole(data.DiscordRoleId).Mention}");
            fieldBuilder.AppendLine($"{Format.Bold(LocalizationGroup.GetText("Percentage", "Percentage quota"))}: {data.Percentage.ToString(LocalizationGroup.CultureInfo)}");
            builder.AddField(LocalizationGroup.GetText("General", "General"), fieldBuilder.ToString());
        }
    }

    /// <inheritdoc/>
    protected override string GetCommandTitle()
    {
        return LocalizationGroup.GetText("CommandTitle", "Commands");
    }

    /// <inheritdoc/>
    public override IReadOnlyList<ReactionData<bool>> GetReactions()
    {
        return _reactions ??= [
                                  new ReactionData<bool>
                                  {
                                      Emote = DiscordEmoteService.GetEditEmote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditInGameNameCommand", "{0} Edit in game name", DiscordEmoteService.GetEditEmote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var inGameName = await RunSubElement<GuildRankInGameNameDialogElement, string>().ConfigureAwait(false);

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
                                  new ReactionData<bool>
                                  {
                                      Emote = DiscordEmoteService.GetEdit2Emote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditDiscordCommand", "{0} Edit discord role", DiscordEmoteService.GetEdit2Emote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var roleId = await RunSubElement<GuildRankDiscordRoleDialogElement, ulong>().ConfigureAwait(false);

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
                                  new ReactionData<bool>
                                  {
                                      Emote = DiscordEmoteService.GetEdit3Emote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("EditPercentageCommand", "{0} Edit percentage", DiscordEmoteService.GetEdit3Emote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var percentage = await RunSubElement<GuildRankPercentageDialogElement, double>().ConfigureAwait(false);

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
                                  new ReactionData<bool>
                                  {
                                      Emote = DiscordEmoteService.GetCrossEmote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("CancelCommand", "{0} Cancel", DiscordEmoteService.GetCrossEmote(CommandContext.Client)),
                                      Func = () => Task.FromResult(false)
                                  }
                              ];
    }

    /// <inheritdoc/>
    protected override bool DefaultFunc()
    {
        return false;
    }

    #endregion // DialogReactionElementBase<bool>
}