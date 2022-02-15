using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 52
    /// </summary>
    public partial class Update52 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DiscordAccountRoleAssignmentHistory");

            migrationBuilder.CreateTable(name: "DiscordHistoricAccountRoleAssignments",
                                         columns: table => new
                                                           {
                                                               Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                                                               ServerId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               RoleId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               AccountId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_DiscordHistoricAccountRoleAssignments",
                                                                           x => new
                                                                                {
                                                                                    x.Date,
                                                                                    x.ServerId,
                                                                                    x.RoleId,
                                                                                    x.AccountId
                                                                                });
                                                      });
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DiscordHistoricAccountRoleAssignments");

            migrationBuilder.CreateTable(name: "DiscordAccountRoleAssignmentHistory",
                                         columns: table => new
                                                           {
                                                               ServerId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               RoleId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               AccountId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_DiscordAccountRoleAssignmentHistory",
                                                                           x => new
                                                                                {
                                                                                    x.ServerId,
                                                                                    x.RoleId,
                                                                                    x.AccountId
                                                                                });
                                                      });
        }
    }
}