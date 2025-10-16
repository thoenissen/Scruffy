using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Selection of the superior rank
/// </summary>
public class GuildRankSuperiorRankDialogElement : DialogEmbedMessageElementBase<int?>
{
    #region Fields

    /// <summary>
    /// Ranks
    /// </summary>
    private Dictionary<int, int?> _ranks;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildRankSuperiorRankDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogEmbedMessageElementBase<ulong>

    /// <inheritdoc/>
    public override EmbedBuilder GetMessage()
    {
        var builder = new EmbedBuilder();
        builder.WithTitle(LocalizationGroup.GetText("ChooseTitle", "Superior rank selection"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseDescription", "Please choose one of the following ranks:"));

        _ranks = new Dictionary<int, int?>();
        var stringBuilder = new StringBuilder();

        var ranksCounter = 1;
        var fieldsCounter = 1;

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var ranks = dbFactory.GetRepository<GuildRankRepository>()
                                 .GetQuery()
                                 .Where(obj => obj.Guild.DiscordServerId == CommandContext.Guild.Id)
                                 .OrderBy(obj => obj.Order)
                                 .Select(obj => new
                                 {
                                     obj.Id,
                                     obj.InGameName,
                                     obj.DiscordRoleId
                                 })
                                 .ToList();

            stringBuilder.Append("`0` - ");
            stringBuilder.Append(LocalizationGroup.GetText("NoSuperior", "No superior rank"));
            stringBuilder.Append('\n');

            _ranks[0] = null;

            foreach (var rank in ranks)
            {
                var currentLine = $"`{ranksCounter}` -  {rank.InGameName} {CommandContext.Guild.GetRole(rank.DiscordRoleId).Mention}\n";

                if (currentLine.Length + stringBuilder.Length > 1024)
                {
                    builder.AddField(LocalizationGroup.GetText("RanksField", "Ranks") + " #" + fieldsCounter, stringBuilder.ToString());
                    stringBuilder.Clear();
                    fieldsCounter++;
                }

                stringBuilder.Append(currentLine);

                _ranks[ranksCounter] = rank.Id;

                ranksCounter++;
            }
        }

        if (stringBuilder.Length == 0)
        {
            stringBuilder.Append("\u200D");
        }

        builder.AddField(LocalizationGroup.GetText("RanksField", "Ranks") + " #" + fieldsCounter, stringBuilder.ToString());

        return builder;
    }

    /// <inheritdoc/>
    public override Task<int?> ConvertMessage(IUserMessage message)
    {
        return Task.FromResult(int.TryParse(message.Content, out var index)
                            && _ranks.TryGetValue(index, out var selectedRank)
                                   ? selectedRank
                                   : null);
    }

    #endregion // DialogEmbedMessageElementBase<ulong>
}