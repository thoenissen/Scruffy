﻿using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// Selection of a template
/// </summary>
public class RaidTemplateSelectionDialogElement : DialogEmbedMessageElementBase<long>
{
    #region Fields

    /// <summary>
    /// Templates
    /// </summary>
    private Dictionary<int, long> _templates;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public RaidTemplateSelectionDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogEmbedMessageElementBase<long>

    /// <inheritdoc/>
    public override EmbedBuilder GetMessage()
    {
        var builder = new EmbedBuilder();
        builder.WithTitle(LocalizationGroup.GetText("ChooseTemplateTitle", "Raid template selection"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseTemplateDescription", "Please choose one of the following templates:"));

        _templates = new Dictionary<int, long>();
        var templatesFieldText = new StringBuilder();

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var mainRoles = dbFactory.GetRepository<RaidDayTemplateRepository>()
                                     .GetQuery()
                                     .Where(obj => obj.IsDeleted == false)
                                     .Select(obj => new
                                                    {
                                                        obj.Id,
                                                        obj.AliasName
                                                    })
                                     .OrderBy(obj => obj.AliasName)
                                     .ToList();

            var i = 1;

            foreach (var role in mainRoles)
            {
                templatesFieldText.Append('`');
                templatesFieldText.Append(i);
                templatesFieldText.Append("` - ");
                templatesFieldText.Append(' ');
                templatesFieldText.Append(role.AliasName);
                templatesFieldText.Append('\n');

                _templates[i] = role.Id;

                i++;
            }

            builder.AddField(LocalizationGroup.GetText("TemplatesField", "Templates"), templatesFieldText.ToString());
        }

        return builder;
    }

    /// <inheritdoc/>
    public override Task<long> ConvertMessage(IUserMessage message)
    {
        return Task.FromResult(int.TryParse(message.Content, out var index) && _templates.TryGetValue(index, out var selectedRoleId) ? selectedRoleId : throw new InvalidOperationException());
    }

    #endregion // DialogEmbedMessageElementBase<long>
}