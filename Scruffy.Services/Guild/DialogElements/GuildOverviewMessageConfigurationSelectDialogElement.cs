using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Overview type selection
/// </summary>
internal class GuildOverviewMessageConfigurationSelectDialogElement : DialogSelectMenuElementBase<GuildOverviewMessageConfigurationSelectDialogElement.MessageType?>
{
    #region Enumerations

    /// <summary>
    /// Message type
    /// </summary>
    public enum MessageType
    {
        Ranking,
    }

    #endregion // Enumerations

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
    public override string GetMessage() => LocalizationGroup.GetText("Message", "Please select of the following types:");

    /// <inheritdoc/>
    public override IReadOnlyList<SelectMenuEntryData<MessageType?>> GetEntries()
    {
        return Enum.GetValues(typeof(MessageType))
                   .OfType<MessageType>()
                   .Select(obj => new SelectMenuEntryData<MessageType?>
                                  {
                                      CommandText = LocalizationGroup.GetText(obj.ToString(), obj.ToString()),
                                      Response = () => Task.FromResult((MessageType?)obj)
                                  })
                   .ToList();
    }

    /// <inheritdoc/>
    protected override MessageType? DefaultFunc() => null;

    #endregion // DialogSelectMenuElementBase
}