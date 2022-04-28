using Discord;
using Discord.Interactions;

namespace Scruffy.Services.Guild.Modals;

/// <summary>
/// Guild stash export
/// </summary>
public class GuildExportStashModalData : IModal
{
    /// <summary>
    /// Custom id
    /// </summary>
    public const string CustomId = "modal;guild;export-stash";

    /// <summary>
    /// Title
    /// </summary>
    public string Title => "Export: Guild stash";

    /// <summary>
    /// Mode
    /// </summary>
    [InputLabel("Mode (raw or sum)")]
    [RequiredInput(false)]
    [ModalTextInput(nameof(Mode), TextInputStyle.Short, "raw", 3, 3)]
    public string Mode { get; set; }

    /// <summary>
    /// Date selection
    /// </summary>
    [InputLabel("Start date selection")]
    [RequiredInput]
    [ModalTextInput(nameof(SinceDate), TextInputStyle.Short, "yyyy-MM-dd", 10, 10)]
    public string SinceDate { get; set; }

    /// <summary>
    /// Date selection
    /// </summary>
    [InputLabel("Start time selection")]
    [RequiredInput(false)]
    [ModalTextInput(nameof(SinceTime), TextInputStyle.Short, "hh:mm", 5, 5)]
    public string SinceTime { get; set; }
}