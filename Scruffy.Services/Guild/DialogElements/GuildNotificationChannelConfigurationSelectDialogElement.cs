using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Channel type selection
/// </summary>
internal class GuildNotificationChannelConfigurationSelectDialogElement : DialogSelectMenuElementBase<GuildNotificationChannelConfigurationSelectDialogElement.ChannelType?>
{
    #region Enumerations

    /// <summary>
    /// Channel type
    /// </summary>
    public enum ChannelType
    {
        SpecialRankNotification,
        CalendarReminderNotification,
        GuildLogNotification,
        GuildRankChangeNotification,
        MessageOfTheDay,
        Calendar,
        UserNotification
    }

    #endregion // Enumerations

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildNotificationChannelConfigurationSelectDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogSelectMenuElementBase

    /// <summary>
    /// Returning the message
    /// </summary>
    /// <returns>Message</returns>
    public override string GetMessage() => LocalizationGroup.GetText("Message", "Please select of the following types:");

    /// <summary>
    /// Returns the select menu entries which should be added to the message
    /// </summary>
    /// <returns>Reactions</returns>
    public override IReadOnlyList<SelectMenuEntryData<ChannelType?>> GetEntries()
    {
        return Enum.GetValues(typeof(ChannelType))
                   .OfType<ChannelType>()
                   .Select(obj => new SelectMenuEntryData<ChannelType?>
                                  {
                                      CommandText = LocalizationGroup.GetText(obj.ToString(), obj.ToString()),
                                      Response = () => Task.FromResult((ChannelType?)obj)
                                  })
                   .ToList();
    }

    /// <summary>
    /// Default case if none of the given buttons is used
    /// </summary>
    /// <returns>Result</returns>
    protected override ChannelType? DefaultFunc() => null;

    #endregion // DialogSelectMenuElementBase
}