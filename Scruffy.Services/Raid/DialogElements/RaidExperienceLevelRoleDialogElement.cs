using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// Acquisition of the experience level discord role
/// </summary>
public class RaidExperienceLevelRoleDialogElement : DialogEmbedMessageElementBase<ulong?>
{
    #region Fields

    /// <summary>
    /// Templates
    /// </summary>
    private Dictionary<int, ulong?> _levels;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public RaidExperienceLevelRoleDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogEmbedMessageElementBase<long>

    /// <inheritdoc/>
    public override EmbedBuilder GetMessage()
    {
        var builder = new EmbedBuilder();
        builder.WithTitle(LocalizationGroup.GetText("ChooseLevelTitle", "Raid experience level role selection"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseLevelDescription", "Please choose one of the following roles:"));

        _levels = [];
        var levelsFieldsText = new StringBuilder();

        levelsFieldsText.Append('`');
        levelsFieldsText.Append(0);
        levelsFieldsText.Append("` - ");
        levelsFieldsText.Append(' ');
        levelsFieldsText.Append(LocalizationGroup.GetText("NoDiscordRole", "No role"));
        levelsFieldsText.Append('\n');

        var i = 1;

        foreach (var role in CommandContext.Guild.Roles)
        {
            levelsFieldsText.Append('`');
            levelsFieldsText.Append(i);
            levelsFieldsText.Append("` - ");
            levelsFieldsText.Append(' ');
            levelsFieldsText.Append(role.Mention);
            levelsFieldsText.Append('\n');

            _levels[i] = role.Id;

            i++;
        }

        builder.AddField(LocalizationGroup.GetText("RolesField", "Roles"), levelsFieldsText.ToString());

        return builder;
    }

    /// <inheritdoc/>
    public override Task<ulong?> ConvertMessage(IUserMessage message)
    {
        return Task.FromResult(int.TryParse(message.Content, out var index) && _levels.TryGetValue(index, out var selectedRoleId) ? selectedRoleId : null);
    }

    #endregion // DialogEmbedMessageElementBase<long>
}