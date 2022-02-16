using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 54
    /// </summary>
    public partial class Update54 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(name: "GuildDiscordActivityPointsAssignments",
                                         columns: table => new
                                                           {
                                                               GuildId = table.Column<long>(type: "bigint", nullable: false),
                                                               Type = table.Column<int>(type: "int", nullable: false),
                                                               RoleId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               Points = table.Column<double>(type: "float", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildDiscordActivityPointsAssignments",
                                                                           x => new
                                                                                {
                                                                                    x.GuildId,
                                                                                    x.Type,
                                                                                    x.RoleId
                                                                                });

                                                          table.ForeignKey(name: "FK_GuildDiscordActivityPointsAssignments_Guilds_GuildId",
                                                                           column: x => x.GuildId,
                                                                           principalTable: "Guilds",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GuildDiscordActivityPointsAssignments");
        }
    }
}