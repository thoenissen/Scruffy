using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 59
    /// </summary>
    public partial class Update59 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(name: "GuildRankAssignments",
                                         columns: table => new
                                                           {
                                                               GuildId = table.Column<long>(type: "bigint", nullable: false),
                                                               UserId = table.Column<long>(type: "bigint", nullable: false),
                                                               RankId = table.Column<int>(type: "int", nullable: false),
                                                               TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildRankAssignments",
                                                                           x => new
                                                                                {
                                                                                    x.GuildId,
                                                                                    x.UserId
                                                                                });

                                                          table.ForeignKey(name: "FK_GuildRankAssignments_Guilds_GuildId",
                                                                           column: x => x.GuildId,
                                                                           principalTable: "Guilds",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey(name: "FK_GuildRankAssignments_Users_UserId",
                                                                           column: x => x.UserId,
                                                                           principalTable: "Users",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
            migrationBuilder.CreateIndex(name: "IX_GuildRankAssignments_UserId", table: "GuildRankAssignments", column: "UserId");
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GuildRankAssignments");
        }
    }
}