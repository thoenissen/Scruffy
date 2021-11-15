namespace Scruffy.Services.Core.Discord.Attributes;

/// <summary>
/// Command of the help overview
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class HelpOverviewCommandAttribute : Attribute
{
    #region Fields

    /// <summary>
    /// Command type
    /// </summary>
    [Flags]
    public enum OverviewType
    {
        /// <summary>
        /// Standard
        /// </summary>
        Standard  = 1,

        /// <summary>
        /// Administrator commands
        /// </summary>
        Administration = 2,

        /// <summary>
        /// Developer commands
        /// </summary>
        Developer = 4
    }

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="type">Type</param>
    public HelpOverviewCommandAttribute(OverviewType type)
    {
        Type = type;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Type
    /// </summary>
    public OverviewType Type { get; }

    #endregion // Properties
}