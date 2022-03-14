using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.GuildWars2.GameData;

/// <summary>
/// Custom recipes
/// </summary>
[Table("GuildWarsCustomRecipeEntries")]
public class GuildWarsCustomRecipeEntryEntity
{
    /// <summary>
    /// Item id
    /// </summary>
    public int ItemId { get; set; }

    /// <summary>
    /// Ingredient item id
    /// </summary>
    public int IngredientItemId { get; set; }

    /// <summary>
    /// Count
    /// </summary>
    public int IngredientCount  { get; set; }
}