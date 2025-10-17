using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 48
    /// </summary>
    public partial class Update48 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_GuildRankCurrentPointsEntity_Guilds_GuildId", table: "GuildRankCurrentPointsEntity");
            migrationBuilder.DropForeignKey(name: "FK_GuildRankCurrentPointsEntity_Users_UserId", table: "GuildRankCurrentPointsEntity");
            migrationBuilder.DropPrimaryKey(name: "PK_GuildRankCurrentPointsEntity", table: "GuildRankCurrentPointsEntity");
            migrationBuilder.RenameTable(name: "GuildRankCurrentPointsEntity", newName: "GuildRankCurrentPoints");
            migrationBuilder.RenameIndex(name: "IX_GuildRankCurrentPointsEntity_UserId", table: "GuildRankCurrentPoints", newName: "IX_GuildRankCurrentPoints_UserId");

            migrationBuilder.AddPrimaryKey(name: "PK_GuildRankCurrentPoints",
                                           table: "GuildRankCurrentPoints",
                                           columns: ["GuildId", "UserId", "Date", "Type"]);

            migrationBuilder.AddForeignKey(name: "FK_GuildRankCurrentPoints_Guilds_GuildId",
                                           table: "GuildRankCurrentPoints",
                                           column: "GuildId",
                                           principalTable: "Guilds",
                                           principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(name: "FK_GuildRankCurrentPoints_Users_UserId",
                                           table: "GuildRankCurrentPoints",
                                           column: "UserId",
                                           principalTable: "Users",
                                           principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_GuildRankCurrentPoints_Guilds_GuildId", table: "GuildRankCurrentPoints");
            migrationBuilder.DropForeignKey(name: "FK_GuildRankCurrentPoints_Users_UserId", table: "GuildRankCurrentPoints");
            migrationBuilder.DropPrimaryKey(name: "PK_GuildRankCurrentPoints", table: "GuildRankCurrentPoints");
            migrationBuilder.RenameTable(name: "GuildRankCurrentPoints", newName: "GuildRankCurrentPointsEntity");
            migrationBuilder.RenameIndex(name: "IX_GuildRankCurrentPoints_UserId", table: "GuildRankCurrentPointsEntity", newName: "IX_GuildRankCurrentPointsEntity_UserId");

            migrationBuilder.AddPrimaryKey(name: "PK_GuildRankCurrentPointsEntity",
                                           table: "GuildRankCurrentPointsEntity",
                                           columns: ["GuildId", "UserId", "Date", "Type"]);

            migrationBuilder.AddForeignKey(name: "FK_GuildRankCurrentPointsEntity_Guilds_GuildId",
                                           table: "GuildRankCurrentPointsEntity",
                                           column: "GuildId",
                                           principalTable: "Guilds",
                                           principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(name: "FK_GuildRankCurrentPointsEntity_Users_UserId",
                                           table: "GuildRankCurrentPointsEntity",
                                           column: "UserId",
                                           principalTable: "Users",
                                           principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);
        }
    }
}