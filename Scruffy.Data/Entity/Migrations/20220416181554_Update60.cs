using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 60
    /// </summary>
    public partial class Update60 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Permissions",
                table: "GuildWarsAccounts",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20,0)");

            migrationBuilder.CreateIndex(
                name: "IX_GuildRankAssignments_RankId",
                table: "GuildRankAssignments",
                column: "RankId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildRankAssignments_GuildRanks_RankId",
                table: "GuildRankAssignments",
                column: "RankId",
                principalTable: "GuildRanks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildRankAssignments_GuildRanks_RankId",
                table: "GuildRankAssignments");

            migrationBuilder.DropIndex(
                name: "IX_GuildRankAssignments_RankId",
                table: "GuildRankAssignments");

            migrationBuilder.AlterColumn<decimal>(
                name: "Permissions",
                table: "GuildWarsAccounts",
                type: "decimal(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
