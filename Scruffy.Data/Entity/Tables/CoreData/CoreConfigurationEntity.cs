using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.CoreData;

/// <summary>
/// Core configuration
/// </summary>
[Table("CoreConfigurations")]
public class CoreConfigurationEntity
{
    #region Properties

    /// <summary>
    /// Key
    /// </summary>
    [Key]
    [StringLength(260)]
    public string Key { get; set; }

    /// <summary>
    /// Value
    /// </summary>
    [StringLength(260)]
    public string Value { get; set; }

    #endregion // Properties
}