namespace Scruffy.Services.Core.Discord.Attributes;

/// <summary>
/// Assigning the dialog element to determinate the data
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DialogElementAssignmentAttribute : Attribute
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dialogElementType">Type of the dialog element</param>
    public DialogElementAssignmentAttribute(Type dialogElementType)
    {
        DialogElementType = dialogElementType;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Type of the dialog element
    /// </summary>
    public Type DialogElementType { get; private set; }

    #endregion // Properties
}