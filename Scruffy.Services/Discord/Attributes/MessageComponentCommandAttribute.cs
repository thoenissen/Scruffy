namespace Scruffy.Services.Discord.Attributes;

/// <summary>
/// Message component command
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class MessageComponentCommandAttribute : Attribute
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="command">Command</param>
    public MessageComponentCommandAttribute(string command)
    {
        Command = command;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Command
    /// </summary>
    public string Command { get; }

    #endregion // Properties
}