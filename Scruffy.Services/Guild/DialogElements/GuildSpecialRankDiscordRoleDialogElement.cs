using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Selection of a discord role
/// </summary>
public class GuildSpecialRankDiscordRoleDialogElement : DialogEmbedMessageElementBase<ulong>
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
    public GuildSpecialRankDiscordRoleDialogElement(LocalizationService localizationService)
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
        builder.WithTitle(LocalizationGroup.GetText("ChooseRoleTitle", "Role selection"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseRoleDescription", "Please choose one of the following roles:"));

        _roles = new Dictionary<int, ulong>();
        var stringBuilder = new StringBuilder();

        var rolesCounter = 1;
        var fieldsCounter = 1;

        foreach (var role in CommandContext.Guild.Roles)
        {
            var currentLine = $"`{rolesCounter}` -  {role.Mention}\n";
            if (currentLine.Length + stringBuilder.Length > 1024)
            {
                builder.AddField(LocalizationGroup.GetText("RolesField", "Roles") + " #" + fieldsCounter, stringBuilder.ToString());
                stringBuilder.Clear();
                fieldsCounter++;
            }

            stringBuilder.Append(currentLine);

            _roles[rolesCounter] = role.Id;

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
    public override Task<ulong> ConvertMessage(IUserMessage message)
    {
        return Task.FromResult(int.TryParse(message.Content, out var index) && _roles.TryGetValue(index, out var selectedRoleId) ? selectedRoleId : throw new InvalidOperationException());
    }

    #endregion // DialogEmbedMessageElementBase<long>
}