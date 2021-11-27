using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.GuildAdministration.DialogElements;

/// <summary>
/// Selection of the superior rank
/// </summary>
public class GuildAdministrationRankSuperiorRankDialogElement : DialogEmbedMessageElementBase<int?>
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
    public GuildAdministrationRankSuperiorRankDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogEmbedMessageElementBase<ulong>

    /// <summary>
    /// Return the message of element
    /// </summary>
    /// <returns>Message</returns>
    public override DiscordEmbedBuilder GetMessage()
    {
        var builder = new DiscordEmbedBuilder();
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
                                 .Select(obj => new
                                                {
                                                    obj.SuperiorId,
                                                    obj.Id,
                                                    obj.InGameName,
                                                    obj.DiscordRoleId
                                                })
                                 .ToList();

            stringBuilder.Append("`0` - ");
            stringBuilder.Append(LocalizationGroup.GetText("NoSuperior", "No superior rank"));
            stringBuilder.Append('\n');

            _ranks[0] = null;

            var rank = ranks.FirstOrDefault(obj => obj.SuperiorId == null);
            while (rank != null)
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

                rank = ranks.FirstOrDefault(obj => obj.SuperiorId == rank.Id);
            }
        }

        if (stringBuilder.Length == 0)
        {
            stringBuilder.Append("\u200D");
        }

        builder.AddField(LocalizationGroup.GetText("RanksField", "Ranks") + " #" + fieldsCounter, stringBuilder.ToString());

        return builder;
    }

    /// <summary>
    /// Converting the response message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override Task<int?> ConvertMessage(DiscordMessage message)
    {
        return Task.FromResult(int.TryParse(message.Content, out var index)
                            && _ranks.TryGetValue(index, out var selectedRank)
                                   ? selectedRank
                                   : null);
    }

    #endregion // DialogEmbedMessageElementBase<ulong>
}