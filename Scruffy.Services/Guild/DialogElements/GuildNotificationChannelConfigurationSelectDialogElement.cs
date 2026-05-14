using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Channel type selection
/// </summary>
internal class GuildNotificationChannelConfigurationSelectDialogElement : DialogSelectMenuElementBase<GuildNotificationChannelType?>
{
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

    /// <inheritdoc/>
    public override string GetMessage()
    {
        return LocalizationGroup.GetText("Message", "Please select of the following types:");
    }

    /// <inheritdoc/>
    public override IReadOnlyList<SelectMenuEntryData<GuildNotificationChannelType?>> GetEntries()
    {
        return Enum.GetValues(typeof(GuildNotificationChannelType))
                   .OfType<GuildNotificationChannelType>()
                   .Select(obj => new SelectMenuEntryData<GuildNotificationChannelType?>
                                  {
                                      CommandText = LocalizationGroup.GetText(obj.ToString(), obj.ToString()),
                                      Response = () => Task.FromResult((GuildNotificationChannelType?)obj)
                                  })
                   .ToList();
    }

    /// <inheritdoc/>
    protected override GuildNotificationChannelType? DefaultFunc()
    {
        return null;
    }

    #endregion // DialogSelectMenuElementBase
}