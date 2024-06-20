using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 31
    /// </summary>
    public partial class Update31 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_RaidCurrentUserPoints_Users_UserId1", "RaidCurrentUserPoints");
            migrationBuilder.DropIndex("IX_RaidCurrentUserPoints_UserId1", "RaidCurrentUserPoints");
            migrationBuilder.DropColumn("UserId1", "RaidCurrentUserPoints");

            migrationBuilder.AddForeignKey("FK_RaidCurrentUserPoints_Users_UserId",
                                           "RaidCurrentUserPoints",
                                           "UserId",
                                           "Users",
                                           principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_RaidCurrentUserPoints_Users_UserId", "RaidCurrentUserPoints");
            migrationBuilder.AddColumn<decimal>("UserId1", "RaidCurrentUserPoints", "decimal(20,0)", nullable: true);
            migrationBuilder.CreateIndex("IX_RaidCurrentUserPoints_UserId1", "RaidCurrentUserPoints", "UserId1");

            migrationBuilder.AddForeignKey("FK_RaidCurrentUserPoints_Users_UserId1",
                                           "RaidCurrentUserPoints",
                                           "UserId1",
                                           "Users",
                                           principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);
        }
    }
}