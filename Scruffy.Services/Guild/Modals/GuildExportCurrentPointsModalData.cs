using Discord;
using Discord.Interactions;

namespace Scruffy.Services.Guild.Modals;

/// <summary>
/// Guild rank points export
/// </summary>
public class GuildExportCurrentPointsModalData : IModal
{
    /// <summary>
    /// Custom id
    /// </summary>
    public const string CustomId = "modal;guild;export-current-points";

    /// <summary>
    /// Title
    /// </summary>
    public string Title => "Export: Current guild rank points";

    /// <summary>
    /// Date selection
    /// </summary>
    [InputLabel("Start date selection")]
    [RequiredInput]
    [ModalTextInput(nameof(SinceDate), TextInputStyle.Short, "yyyy-MM-dd", 10, 10)]
    public string SinceDate { get; set; }
}