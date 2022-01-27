
using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Selection of the in game role
/// </summary>
public class GuildRankInGameNameDialogElement : DialogEmbedMessageElementBase<string>
{
    #region Fields

    /// <summary>
    /// Ranks
    /// </summary>
    private Dictionary<int, string> _ranks;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildRankInGameNameDialogElement(LocalizationService localizationService)
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
        builder.WithTitle(LocalizationGroup.GetText("ChooseTitle", "In game rank selection"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseDescription", "Please choose one of the following ranks:"));

        _ranks = new Dictionary<int, string>();
        var stringBuilder = new StringBuilder();

        var ranksCounter = 1;
        var fieldsCounter = 1;

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var guild = dbFactory.GetRepository<GuildRepository>()
                                 .GetQuery()
                                 .Where(obj => obj.DiscordServerId == CommandContext.Guild.Id)
                                 .Select(obj => new
                                                {
                                                    obj.GuildId,
                                                    obj.ApiKey
                                                })
                                 .First();

            var connector = new GuidWars2ApiConnector(guild.ApiKey);

            foreach (var rank in connector.GetGuildRanks(guild.GuildId)
                                          .Result
                                          .OrderBy(obj => obj.Order))
            {
                var currentLine = $"`{ranksCounter}` -  {rank.Id}\n";
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

    /// <summary>
    /// Converting the response message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override Task<string> ConvertMessage(DiscordMessage message)
    {
        return Task.FromResult(int.TryParse(message.Content, out var index)
                            && _ranks.TryGetValue(index, out var selectedRank)
                                   ? selectedRank
                                   : throw new InvalidOperationException());
    }

    #endregion // DialogEmbedMessageElementBase<ulong>
}