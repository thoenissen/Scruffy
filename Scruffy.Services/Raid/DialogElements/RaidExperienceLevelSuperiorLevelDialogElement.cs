﻿using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// Selection of a template
/// </summary>
public class RaidExperienceLevelSuperiorLevelDialogElement : DialogEmbedMessageElementBase<long?>
{
    #region Fields

    /// <summary>
    /// Templates
    /// </summary>
    private Dictionary<int, long?> _levels;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public RaidExperienceLevelSuperiorLevelDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogEmbedMessageElementBase<long>

    /// <summary>
    /// Return the message of element
    /// </summary>
    /// <returns>Message</returns>
    public override DiscordEmbedBuilder GetMessage()
    {
        var builder = new DiscordEmbedBuilder();
        builder.WithTitle(LocalizationGroup.GetText("ChooseLevelTitle", "Raid experience level selection"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseLevelDescription", "Please choose one of the following experience levels:"));

        _levels = new Dictionary<int, long?>();
        var levelsFieldsText = new StringBuilder();

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var mainRoles = dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                     .GetQuery()
                                     .Where(obj => obj.IsDeleted == false)
                                     .Select(obj => new
                                                    {
                                                        obj.Id,
                                                        obj.Description
                                                    })
                                     .OrderBy(obj => obj.Description)
                                     .ToList();

            levelsFieldsText.Append('`');
            levelsFieldsText.Append(0);
            levelsFieldsText.Append("` - ");
            levelsFieldsText.Append(' ');
            levelsFieldsText.Append(LocalizationGroup.GetText("NoExperienceLevel", "No superior level"));
            levelsFieldsText.Append('\n');

            var i = 1;
            foreach (var role in mainRoles)
            {
                levelsFieldsText.Append('`');
                levelsFieldsText.Append(i);
                levelsFieldsText.Append("` - ");
                levelsFieldsText.Append(' ');
                levelsFieldsText.Append(role.Description);
                levelsFieldsText.Append('\n');

                _levels[i] = role.Id;

                i++;
            }

            builder.AddField(LocalizationGroup.GetText("LevelsField", "Levels"), levelsFieldsText.ToString());
        }

        return builder;
    }

    /// <summary>
    /// Converting the response message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override Task<long?> ConvertMessage(DiscordMessage message)
    {
        return Task.FromResult(int.TryParse(message.Content, out var index) && _levels.TryGetValue(index, out var selectedRoleId) ? selectedRoleId : null);
    }

    #endregion // DialogEmbedMessageElementBase<long>
}