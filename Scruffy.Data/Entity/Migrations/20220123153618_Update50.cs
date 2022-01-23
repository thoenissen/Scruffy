using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update
    /// </summary>
    public partial class Update50 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(name: "GuildWarsAccountRankingData",
                                         columns: table => new
                                                           {
                                                               AccountName = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                                                               Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                                                               AchievementPoints = table.Column<long>(type: "bigint", nullable: true)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildWarsAccountRankingData",
                                                                           x => new
                                                                                {
                                                                                    x.AccountName,
                                                                                    x.Date
                                                                                });
                                                      });

            migrationBuilder.CreateTable(name: "GuildWarsAccountRankingGuildData",
                                         columns: table => new
                                                           {
                                                               GuildId = table.Column<long>(type: "bigint", nullable: false),
                                                               AccountName = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                                                               Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                                                               RepresentationPercentage = table.Column<int>(type: "int", nullable: true),
                                                               DonationValue = table.Column<decimal>(type: "decimal(20,0)", nullable: true)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildWarsAccountRankingGuildData",
                                                                           x => new
                                                                                {
                                                                                    x.AccountName,
                                                                                    x.GuildId,
                                                                                    x.Date
                                                                                });
                                                      });
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DiscordAccountRoleAssignmentHistory");
            migrationBuilder.DropTable(name: "GuildWarsAccountRankingData");
            migrationBuilder.DropTable(name: "GuildWarsAccountRankingGuildData");
        }
    }
}