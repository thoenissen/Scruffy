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
        AddReportGroup(group.AsText(), content);
    }

    /// <summary>
    /// Add a group of DPS reports
    /// </summary>
    /// <param name="group">The DPS report group</param>
    /// <param name="content">The content of the report group</param>
    public void AddReportGroup(string group, string content)
    {
        if (_reportGroupsInRow > 1)
        {
            AddField("\u200b", "\u200b", false);
            _reportGroupsInRow = 0;
        }

        AddField(group, content, true);
        ++_reportGroupsInRow;
    }
}