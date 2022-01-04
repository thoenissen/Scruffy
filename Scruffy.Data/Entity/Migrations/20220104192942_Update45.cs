using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 45
    /// </summary>
    public partial class Update45 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsValueReducingActivated", table: "GuildWarsItems");

            migrationBuilder.AddColumn<int>(name: "CustomValueThreshold", table: "GuildWarsItems", type: "int", nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "CustomValueValidDate", table: "GuildWarsItems", type: "datetime2", nullable: true);

            migrationBuilder.AddColumn<bool>(name: "IsProcessed",
                                             table: "GuildLogEntries",
                                             type: "bit",
                                             nullable: false,
                                             defaultValue: false);

            migrationBuilder.CreateTable(name: "GuildRankCurrentPointsEntity",
                                         columns: table => new
                                                           {
                                                               GuildId = table.Column<long>(type: "bigint", nullable: false),
                                                               UserId = table.Column<long>(type: "bigint", nullable: false),
                                                               Type = table.Column<int>(type: "int", nullable: false),
                                                               Points = table.Column<double>(type: "float", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildRankCurrentPointsEntity",
                                                                           x => new
                                                                                {
                                                                                    x.GuildId,
                                                                                    x.UserId,
                                                                                    x.Type
                                                                                });

                                                          table.ForeignKey(name: "FK_GuildRankCurrentPointsEntity_Guilds_GuildId",
                                                                           column: x => x.GuildId,
                                                                           principalTable: "Guilds",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey(name: "FK_GuildRankCurrentPointsEntity_Users_UserId",
                                                                           column: x => x.UserId,
                                                                           principalTable: "Users",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
            migrationBuilder.CreateIndex(name: "IX_GuildRankCurrentPointsEntity_UserId", table: "GuildRankCurrentPointsEntity", column: "UserId");
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GuildRankCurrentPointsEntity");
            migrationBuilder.DropColumn(name: "CustomValueThreshold", table: "GuildWarsItems");
            migrationBuilder.DropColumn(name: "CustomValueValidDate", table: "GuildWarsItems");
            migrationBuilder.DropColumn(name: "IsProcessed", table: "GuildLogEntries");

            migrationBuilder.AddColumn<bool>(name: "IsValueReducingActivated",
                                             table: "GuildWarsItems",
                                             type: "bit",
                                             nullable: false,
                                             defaultValue: false);
        }
    }
}