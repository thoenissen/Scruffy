using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 33
    /// </summary>
    public partial class Update33 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(name: "GuildWarsItems",
                                         columns: table => new
                                                           {
                                                               ItemId = table.Column<int>(type: "int", nullable: false),
                                                               Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                               Type = table.Column<int>(type: "int", nullable: false),
                                                               VendorValue = table.Column<long>(type: "bigint", nullable: true),
                                                               CustomValue = table.Column<long>(type: "bigint", nullable: true),
                                                               IsValueReducingActivated = table.Column<bool>(type: "bit", nullable: false)
                                                           },
                                         constraints: table => table.PrimaryKey("PK_GuildWarsItems", x => x.ItemId));

            migrationBuilder.CreateTable(name: "GuildWarsItemGuildUpgradeConversions",
                                         columns: table => new
                                                           {
                                                               ItemId = table.Column<int>(type: "int", nullable: false),
                                                               UpgradeId = table.Column<long>(type: "bigint", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildWarsItemGuildUpgradeConversions",
                                                                           x => new
                                                                                {
                                                                                    x.ItemId,
                                                                                    x.UpgradeId
                                                                                });

                                                          table.ForeignKey(name: "FK_GuildWarsItemGuildUpgradeConversions_GuildWarsItems_ItemId",
                                                                           column: x => x.ItemId,
                                                                           principalTable: "GuildWarsItems",
                                                                           principalColumn: "ItemId",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GuildWarsItemGuildUpgradeConversions");
            migrationBuilder.DropTable(name: "GuildWarsItems");
        }
    }
}