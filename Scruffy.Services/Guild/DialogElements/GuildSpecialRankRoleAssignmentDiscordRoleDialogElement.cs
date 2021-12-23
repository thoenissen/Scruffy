using DSharpPlus.Entities;

using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Selection of a discord role
/// </summary>
public class GuildSpecialRankRoleAssignmentDiscordRoleDialogElement : DialogEmbedMessageElementBase<ulong>
{
    #region Fields

    /// <summary>
    /// Roles
    /// </summary>
    private Dictionary<int, ulong> _roles;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildSpecialRankRoleAssignmentDiscordRoleDialogElement(LocalizationService localizationService)
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
        builder.WithTitle(LocalizationGroup.GetText("ChooseRoleTitle", "Role selection"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseRoleDescription", "Please choose one of the following roles:"));

        _roles = new Dictionary<int, ulong>();
        var stringBuilder = new StringBuilder();

        var rolesCounter = 1;
        var fieldsCounter = 1;

        foreach (var (key, value) in CommandContext.Guild.Roles)
        {
            var currentLine = $"`{rolesCounter}` -  {value.Mention}\n";
            if (currentLine.Length + stringBuilder.Length > 1024)
            {
                builder.AddField(LocalizationGroup.GetText("RolesField", "Roles") + " #" + fieldsCounter, stringBuilder.ToString());
                stringBuilder.Clear();
                fieldsCounter++;
            }

            stringBuilder.Append(currentLine);

            _roles[rolesCounter] = key;

            rolesCounter++;
        }

        if (stringBuilder.Length == 0)
        {
            stringBuilder.Append("\u200D");
        }

        builder.AddField(LocalizationGroup.GetText("RolesField", "Roles") + " #" + fieldsCounter, stringBuilder.ToString());

        return builder;
    }

    /// <summary>
    /// Converting the response message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override Task<ulong> ConvertMessage(DiscordMessage message)
    {
        return Task.FromResult(int.TryParse(message.Content, out var index) && _roles.TryGetValue(index, out var selectedRoleId) ? selectedRoleId : throw new InvalidOperationException());
    }

    #endregion // DialogEmbedMessageElementBase<ulong>
}