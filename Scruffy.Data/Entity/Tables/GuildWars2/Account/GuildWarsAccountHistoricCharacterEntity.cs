using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.GuildWars2.Account;

/// <summary>
/// Characters of an account
/// </summary>
[Table("GuildWarsAccountHistoricCharacters")]
public class GuildWarsAccountHistoricCharacterEntity
{
    #region Properties

    /// <summary>
    /// Date
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Account name
    /// </summary>
    [StringLength(42)]
    public string AccountName { get; set; }

    /// <summary>
    /// Character name
    /// </summary>
    public string CharacterName { get; set; }

    /// <summary>
    /// Id of the guild
    /// </summary>
    public string GuildId { get; set; }

    #endregion // Properties
}