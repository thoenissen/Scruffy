using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Services.Fractals.DialogElements.Forms;

/// <summary>
/// Fractal lfg creation
/// </summary>
public class FractalLfgCreationFormData
{
    /// <summary>
    /// Title
    /// </summary>
    [DialogElementAssignment(typeof(FractalLfgCreationTitleDialogElement))]
    public string Title { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    [DialogElementAssignment(typeof(FractalLfgCreationDescriptionDialogElement))]
    public string Description { get; set; }

    /// <summary>
    /// Alias name
    /// </summary>
    [DialogElementAssignment(typeof(FractalLfgCreationAliasNameDialogElement))]
    public string AliasName { get; set; }
}