using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 55
    /// </summary>
    public partial class Update55 : Migration
    {
        /// <summary>
        /// Upgrrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DiscordHistoryRoleAssignments");
            migrationBuilder.AddColumn<string>(name: "GitHubAccount", table: "Users", type: "nvarchar(max)", nullable: true);

            migrationBuilder.CreateTable(name: "GitHubCommitEntity",
                                         columns: table => new
                                                           {
                                                               Sha = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                                                               Author = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                               Committer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                               TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                                                           },
                                         constraints: table => table.PrimaryKey("PK_GitHubCommitEntity", x => x.Sha));
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GitHubCommitEntity");
            migrationBuilder.DropColumn(name: "GitHubAccount", table: "Users");

            migrationBuilder.CreateTable(name: "DiscordHistoryRoleAssignments",
                                         columns: table => new
                                                           {
                                                               Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                                                               ServerId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               UserId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               RoleId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_DiscordHistoryRoleAssignments",
                                                                           x => new
                                                                                {
                                                                                    x.Date,
                                                                                    x.ServerId,
                                                                                    x.UserId,
                                                                                    x.RoleId
                                                                                });
                                                      });
        }
    }
}