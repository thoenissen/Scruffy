namespace Scruffy.Data.Services.Guild;

/// <summary>
/// Welcome direct message
/// </summary>
public class WelcomeDirectMessageData
{
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Fields
    /// </summary>
    public List<(string Title, string Text)> Fields { get; set; }

    /// <summary>
    /// Footer
    /// </summary>
    public string Footer { get; set; }
}