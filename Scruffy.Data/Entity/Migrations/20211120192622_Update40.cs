using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 40
    /// </summary>
    public partial class Update40 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(name: "DailyAchievementPoints", table: "GuildWarsAccounts", type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(name: "MonthlyAchievementPoints", table: "GuildWarsAccounts", type: "int", nullable: true);
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DailyAchievementPoints", table: "GuildWarsAccounts");
            migrationBuilder.DropColumn(name: "MonthlyAchievementPoints", table: "GuildWarsAccounts");
        }
    }
}