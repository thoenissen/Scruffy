using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Selection of a special rank
/// </summary>
public class GuildSpecialRankSelectionDialogElement : DialogEmbedMessageElementBase<long>
{
    #region Fields

    /// <summary>
    /// Ranks
    /// </summary>
    private Dictionary<int, long> _ranks;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildSpecialRankSelectionDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogEmbedMessageElementBase<long>

    /// <inheritdoc/>
    public override EmbedBuilder GetMessage()
    {
        var builder = new EmbedBuilder();
        builder.WithTitle(LocalizationGroup.GetText("ChooseSpecialRankTitle", "Special rank selection"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseSpecialRankDescription", "Please choose one of the following special ranks:"));

        _ranks = new Dictionary<int, long>();
        var levelsFieldsText = new StringBuilder();

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var mainRoles = dbFactory.GetRepository<GuildSpecialRankConfigurationRepository>()
                                     .GetQuery()
                                     .Where(obj => obj.Guild.DiscordServerId == CommandContext.Guild.Id
                                                && obj.IsDeleted == false)
                                     .Select(obj => new
                                                    {
                                                        obj.Id,
                                                        obj.Description
                                                    })
                                     .OrderBy(obj => obj.Description)
                                     .ToList();

            var i = 1;

            foreach (var role in mainRoles)
            {
                levelsFieldsText.Append('`');
                levelsFieldsText.Append(i);
                levelsFieldsText.Append("` - ");
                levelsFieldsText.Append(' ');
                levelsFieldsText.Append(role.Description);
                levelsFieldsText.Append('\n');

                _ranks[i] = role.Id;

                i++;
            }

            builder.AddField(LocalizationGroup.GetText("RanksField", "Ranks"), levelsFieldsText.ToString());
        }

        return builder;
    }

    /// <inheritdoc/>
    public override Task<long> ConvertMessage(IUserMessage message)
    {
        return Task.FromResult(int.TryParse(message.Content, out var index) && _ranks.TryGetValue(index, out var selected) ? selected : throw new InvalidOperationException());
    }

    #endregion // DialogEmbedMessageElementBase<long>
}