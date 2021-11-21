using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 41
    /// </summary>
    public partial class Update41 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(name: "GuildRanks",
                                         columns: table => new
                                                           {
                                                               Id = table.Column<int>(type: "int", nullable: false)
                                                                         .Annotation("SqlServer:Identity", "1, 1"),
                                                               GuildId = table.Column<long>(type: "bigint", nullable: false),
                                                               SuperiorId = table.Column<int>(type: "int", nullable: true),
                                                               DiscordRoleId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               InGameName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                               Percentage = table.Column<double>(type: "float", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildRanks", x => x.Id);

                                                          table.ForeignKey(name: "FK_GuildRanks_GuildRanks_SuperiorId",
                                                                           column: x => x.SuperiorId,
                                                                           principalTable: "GuildRanks",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey(name: "FK_GuildRanks_Guilds_GuildId",
                                                                           column: x => x.GuildId,
                                                                           principalTable: "Guilds",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
            migrationBuilder.CreateIndex(name: "IX_GuildRanks_GuildId", table: "GuildRanks", column: "GuildId");
            migrationBuilder.CreateIndex(name: "IX_GuildRanks_SuperiorId", table: "GuildRanks", column: "SuperiorId");
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GuildRanks");
        }
    }
}