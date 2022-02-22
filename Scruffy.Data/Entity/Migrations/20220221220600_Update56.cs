using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 56
    /// </summary>
    public partial class Update56 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(name: "PK_GitHubCommitEntity", table: "GitHubCommitEntity");
            migrationBuilder.RenameTable(name: "GitHubCommitEntity", newName: "GitHubCommits");
            migrationBuilder.AddColumn<bool>(name: "IsCustomValueThresholdActivated",
                                             table: "GuildWarsItems",
                                             type: "bit",
                                             nullable: false,
                                             defaultValue: false);
            migrationBuilder.Sql("UPDATE [GuildWarsItems] SET [IsCustomValueThresholdActivated] = 1 WHERE [CustomValueThreshold] IS NOT NULL");
            migrationBuilder.DropColumn(name: "CustomValueThreshold", table: "GuildWarsItems");
            migrationBuilder.AddPrimaryKey(name: "PK_GitHubCommits", table: "GitHubCommits", column: "Sha");
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(name: "PK_GitHubCommits", table: "GitHubCommits");
            migrationBuilder.DropColumn(name: "IsCustomValueThresholdActivated", table: "GuildWarsItems");
            migrationBuilder.RenameTable(name: "GitHubCommits", newName: "GitHubCommitEntity");
            migrationBuilder.AddColumn<int>(name: "CustomValueThreshold", table: "GuildWarsItems", type: "int", nullable: true);
            migrationBuilder.AddPrimaryKey(name: "PK_GitHubCommitEntity", table: "GitHubCommitEntity", column: "Sha");
        }
    }
}