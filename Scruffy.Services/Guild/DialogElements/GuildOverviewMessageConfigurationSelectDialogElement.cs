using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Overview type selection
/// </summary>
internal class GuildOverviewMessageConfigurationSelectDialogElement : DialogSelectMenuElementBase<GuildOverviewMessageType?>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildOverviewMessageConfigurationSelectDialogElement(LocalizationService localizationService)
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
    public override IReadOnlyList<SelectMenuEntryData<GuildOverviewMessageType?>> GetEntries()
    {
        return Enum.GetValues(typeof(GuildOverviewMessageType))
                   .OfType<GuildOverviewMessageType>()
                   .Select(obj => new SelectMenuEntryData<GuildOverviewMessageType?>
                                  {
                                      CommandText = LocalizationGroup.GetText(obj.ToString(), obj.ToString()),
                                      Response = () => Task.FromResult((GuildOverviewMessageType?)obj)
                                  })
                   .ToList();
    }

    /// <inheritdoc/>
    protected override GuildOverviewMessageType? DefaultFunc()
    {
        return null;
    }

    #endregion // DialogSelectMenuElementBase
}