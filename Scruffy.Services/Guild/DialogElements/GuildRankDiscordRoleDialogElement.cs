﻿using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Selection of the discord role
/// </summary>
public class GuildRankDiscordRoleDialogElement : DialogEmbedMessageElementBase<ulong>
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
    public GuildRankDiscordRoleDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogEmbedMessageElementBase<ulong>

    /// <inheritdoc/>
    public override EmbedBuilder GetMessage()
    {
        var builder = new EmbedBuilder();
        builder.WithTitle(LocalizationGroup.GetText("ChooseTitle", "Role selection"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseDescription", "Please choose one of the following roles:"));

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

    /// <inheritdoc/>
    public override Task<ulong> ConvertMessage(IUserMessage message)
    {
        return Task.FromResult(int.TryParse(message.Content, out var index) && _roles.TryGetValue(index, out var selectedRoleId) ? selectedRoleId : throw new InvalidOperationException());
    }

    #endregion // DialogEmbedMessageElementBase<ulong>
}