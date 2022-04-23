using Discord;
using Discord.Interactions;

namespace Scruffy.Services.Account.Modals;

/// <summary>
/// Personal data
/// </summary>
public class PersonalDataModal : IModal
{
    /// <summary>
    /// Custom id
    /// </summary>
    public const string CustomId = "modal;acccount;personal-data";

    /// <summary>
    /// Title
    /// </summary>
    public string Title => "Personal data";

    /// <summary>
    /// Name
    /// </summary>
    [InputLabel("Name")]
    [RequiredInput(false)]
    [ModalTextInput(nameof(Name))]
    public string Name { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    [InputLabel("Birthday")]
    [RequiredInput(false)]
    [ModalTextInput(nameof(Birthday), TextInputStyle.Short, "yyyy-MM-dd", 10, 10)]
    public string Birthday { get; set; }
}