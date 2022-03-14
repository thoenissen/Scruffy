using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 57
    /// </summary>
    public partial class Update57 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(name: "GuildDonations",
                                         columns: table => new
                                                           {
                                                               GuildId = table.Column<long>(type: "bigint", nullable: false),
                                                               LogEntryId = table.Column<int>(type: "int", nullable: false),
                                                               Value = table.Column<long>(type: "bigint", nullable: false),
                                                               IsThresholdRelevant = table.Column<bool>(type: "bit", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildDonations",
                                                                           x => new
                                                                                {
                                                                                    x.GuildId,
                                                                                    x.LogEntryId
                                                                                });
                                                      });

            migrationBuilder.CreateTable(name: "GuildWarsCustomRecipeEntries",
                                         columns: table => new
                                                           {
                                                               ItemId = table.Column<int>(type: "int", nullable: false),
                                                               IngredientItemId = table.Column<int>(type: "int", nullable: false),
                                                               IngredientCount = table.Column<int>(type: "int", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildWarsCustomRecipeEntries",
                                                                           x => new
                                                                                {
                                                                                    x.ItemId,
                                                                                    x.IngredientItemId
                                                                                });
                                                      });
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GuildDonations");
            migrationBuilder.DropTable(name: "GuildWarsCustomRecipeEntries");
        }
    }
}