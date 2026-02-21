using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;

namespace Scruffy.WebApp.Components.Services.DpsReports;

/// <summary>
/// Shared UI helper methods for DPS report pages
/// </summary>
public class DpsReportVisualizer : LocatedServiceBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public DpsReportVisualizer(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the skill level based on the uptime percentage
    /// </summary>
    /// <param name="uptime">Uptime</param>
    /// <returns>Skill-Level CSS class</returns>
    public string GetSkillLevelFromUptime(double? uptime)
    {
        if (uptime > 80.00D)
        {
            return "skill-level-2";
        }

        if (uptime > 50.00D)
        {
            return "skill-level-1";
        }

        return "skill-level-0";
    }

    /// <summary>
    /// Gets the skill level for a mechanic based on the count
    /// </summary>
    /// <param name="count">Count</param>
    /// <returns>Skill-Level CSS class</returns>
    public string GetSkillLevelForMechanic(int count)
    {
        if (count <= 1)
        {
            return "skill-level-2";
        }

        if (count <= 3)
        {
            return "skill-level-1";
        }

        return "skill-level-0";
    }

    /// <summary>
    /// Gets the skill level from player DPS
    /// </summary>
    /// <param name="dps">DPS</param>
    /// <returns>Skill-Level CSS class</returns>
    public string GetSkillLevelFromDps(int? dps)
    {
        if (dps < 10_000)
        {
            return "skill-level-0";
        }

        if (dps < 20_000)
        {
            return "skill-level-1";
        }

        return "skill-level-2";
    }

    /// <summary>
    /// Gets the CSS class for the boss item based on its status
    /// </summary>
    /// <param name="isSuccessful">Success status</param>
    /// <returns>CSS class name</returns>
    public string GetBossStatusClass(bool? isSuccessful)
    {
        return isSuccessful switch
               {
                   true => "boss-item-success",
                   false => "boss-item-failure",
                   null => "boss-item-unknown"
               };
    }

    /// <summary>
    /// Gets the CSS class for the success indicator based on status
    /// </summary>
    /// <param name="isSuccessful">Success status</param>
    /// <returns>CSS class name</returns>
    public string GetIndicatorClass(bool? isSuccessful)
    {
        return isSuccessful switch
               {
                   true => "indicator-success",
                   false => "indicator-failure",
                   null => "indicator-unknown"
               };
    }

    /// <summary>
    /// Gets the tooltip text based on the success status
    /// </summary>
    /// <param name="isSuccessful">Success status</param>
    /// <returns>Tooltip text</returns>
    public string GetStatusTooltip(bool? isSuccessful)
    {
        return isSuccessful switch
               {
                   true => LocalizationGroup.GetText("Success", "Successfully defeated"),
                   false => LocalizationGroup.GetText("Unsuccessful", "Unsuccessfully attempted"),
                   null => LocalizationGroup.GetText("NotAttempted", "Not attempted")
               };
    }

    #endregion // Methods
}