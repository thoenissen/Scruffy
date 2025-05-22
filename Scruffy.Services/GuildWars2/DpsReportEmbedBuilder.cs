using Discord;

using Scruffy.Data.Enumerations.DpsReport;

namespace Scruffy.Services.GuildWars2;

/// <summary>
/// Embed Builder extension for DPS reports
/// </summary>
public class DpsReportEmbedBuilder : EmbedBuilder
{
    /// <summary>
    /// Amount of report groups in the current row
    /// </summary>
    private int _reportGroupsInRow;

    /// <summary>
    /// Adds a sub title field
    /// </summary>
    /// <param name="title">The title to use</param>
    public void AddSubTitle(string title)
    {
        AddField("\u200b", title);
        _reportGroupsInRow = 0;
    }

    /// <summary>
    /// Add a group of DPS reports
    /// </summary>
    /// <param name="group">The DPS report group</param>
    /// <param name="content">The content of the report group</param>
    public void AddReportGroup(DpsReportGroup group, string content)
    {
        AddReportGroup(group.AsText(), content, true);
    }

    /// <summary>
    /// Add a group of DPS reports
    /// </summary>
    /// <param name="group">The DPS report group</param>
    /// <param name="content">The content of the report group</param>
    /// <param name="isInsertEmptyColumn">Should an empty row inserted if needed?</param>
    public void AddReportGroup(string group, string content, bool isInsertEmptyColumn)
    {
        if (_reportGroupsInRow > 1)
        {
            if (isInsertEmptyColumn)
            {
                AddField("\u200b", "\u200b", false);
            }

            _reportGroupsInRow = 0;
        }

        if (content.Length > 1024)
        {
            content = content[..1024];
        }

        AddField(group, content, true);

        ++_reportGroupsInRow;
    }
}