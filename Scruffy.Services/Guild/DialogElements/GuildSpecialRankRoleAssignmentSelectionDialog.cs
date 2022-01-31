using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Selection of a special rank role assignment
/// </summary>
public class GuildSpecialRankRoleAssignmentSelectionDialog : DialogEmbedMessageElementBase<ulong>
{
    #region Fields

    /// <summary>
    /// Assignments
    /// </summary>
    private Dictionary<int, ulong> _ranks;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildSpecialRankRoleAssignmentSelectionDialog(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogEmbedMessageElementBase<long>

    /// <summary>
    /// Return the message of element
    /// </summary>
    /// <returns>Message</returns>
    public override EmbedBuilder GetMessage()
    {
        var builder = new EmbedBuilder();
        builder.WithTitle(LocalizationGroup.GetText("ChooseAssignmentTitle", "Assignment selection"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseAssignmentDescription", "Please choose one of the following assignments:"));

        _ranks = new Dictionary<int, ulong>();
        var levelsFieldsText = new StringBuilder();

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var rankId = DialogContext.GetValue<long>("RankId");

            var mainRoles = dbFactory.GetRepository<GuildSpecialRankRoleAssignmentRepository>()
                                     .GetQuery()
                                     .Where(obj => obj.ConfigurationId == rankId)
                                     .Select(obj => new
                                                    {
                                                        obj.DiscordRoleId
                                                    })
                                     .ToList();

            var i = 1;
            foreach (var role in mainRoles)
            {
                levelsFieldsText.Append('`');
                levelsFieldsText.Append(i);
                levelsFieldsText.Append("` - ");
                levelsFieldsText.Append(' ');
                levelsFieldsText.Append(CommandContext.Guild.GetRole(role.DiscordRoleId).Mention);
                levelsFieldsText.Append('\n');

                _ranks[i] = role.DiscordRoleId;

                i++;
            }

            builder.AddField(LocalizationGroup.GetText("AssignmentsField", "Assignments"), levelsFieldsText.ToString());
        }

        return builder;
    }

    /// <summary>
    /// Converting the response message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override Task<ulong> ConvertMessage(IUserMessage message)
    {
        return Task.FromResult(int.TryParse(message.Content, out var index) && _ranks.TryGetValue(index, out var selected) ? selected : throw new InvalidOperationException());
    }

    #endregion // DialogEmbedMessageElementBase<long>
}