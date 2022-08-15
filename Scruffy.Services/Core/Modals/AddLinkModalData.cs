using Discord;
using Discord.Interactions;

namespace Scruffy.Services.Core.Modals;

/// <summary>
/// Message editing: Add link
/// </summary>
public class AddLinkModalData : IModal
{
    /// <summary>
    /// Custom id
    /// </summary>
    public const string CustomIdPrefix = "modal;utility;add_link";

    /// <summary>
    /// Title
    /// </summary>
    public string Title => "Message editing: Link input";

    /// <summary>
    /// Name
    /// </summary>
    [InputLabel("Name")]
    [RequiredInput]
    [ModalTextInput(nameof(Name), TextInputStyle.Short, "Name")]
    public string Name { get; set; }

    /// <summary>
    /// Link
    /// </summary>
    [InputLabel("Link")]
    [RequiredInput]
    [ModalTextInput(nameof(Link), TextInputStyle.Short, "https://wwww.todo.de")]
    public string Link { get; set; }

    /// <summary>
    /// Get custom id
    /// </summary>
    /// <param name="channelId">Channel id</param>
    /// <param name="messageId">message id</param>
    /// <returns>Custom id</returns>
    public static string GetCustomId(ulong channelId, ulong messageId) => $"{CustomIdPrefix};{channelId};{messageId}";
}