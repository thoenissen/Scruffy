using Discord;
using Discord.Interactions;

namespace Scruffy.Services.Core.Modals;

/// <summary>
/// Message editing: Add link
/// </summary>
public class AddLinkModalData : IModal
{
    #region Constants

    /// <summary>
    /// Custom id
    /// </summary>
    public const string CustomIdPrefix = "modal;utility;add_link";

    #endregion // Constants

    #region Properties

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

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Get custom id
    /// </summary>
    /// <param name="channelId">Channel id</param>
    /// <param name="messageId">message id</param>
    /// <returns>Custom id</returns>
    public static string GetCustomId(ulong channelId, ulong messageId)
    {
        return $"{CustomIdPrefix};{channelId};{messageId}";
    }

    #endregion // Methods

    #region IModal

    /// <summary>
    /// Title
    /// </summary>
    public string Title => "Message editing: Link input";

    #endregion // IModal
}