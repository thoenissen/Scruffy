using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 51
    /// </summary>
    public partial class Update51 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GuildWarsGuildMembers");

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

            migrationBuilder.CreateTable(name: "GuildWarsAccountHistoricCharacters",
                                         columns: table => new
                                                           {
                                                               Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                                                               AccountName = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                                                               CharacterName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                                                               GuildId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildWarsAccountHistoricCharacters",
                                                                           x => new
                                                                                {
                                                                                    x.Date,
                                                                                    x.AccountName,
                                                                                    x.CharacterName
                                                                                });
                                                      });

            migrationBuilder.CreateTable(name: "GuildWarsGuildHistoricMembers",
                                         columns: table => new
                                                           {
                                                               Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                                                               GuildId = table.Column<long>(type: "bigint", nullable: false),
                                                               Name = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                                                               Rank = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                               JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildWarsGuildHistoricMembers",
                                                                           x => new
                                                                                {
                                                                                    x.Date,
                                                                                    x.GuildId,
                                                                                    x.Name
                                                                                });

                                                          table.ForeignKey(name: "FK_GuildWarsGuildHistoricMembers_Guilds_GuildId",
                                                                           column: x => x.GuildId,
                                                                           principalTable: "Guilds",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
            migrationBuilder.CreateIndex(name: "IX_GuildWarsGuildHistoricMembers_GuildId", table: "GuildWarsGuildHistoricMembers", column: "GuildId");
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DiscordHistoryRoleAssignments");
            migrationBuilder.DropTable(name: "GuildWarsAccountHistoricCharacters");
            migrationBuilder.DropTable(name: "GuildWarsGuildHistoricMembers");

            migrationBuilder.CreateTable(name: "GuildWarsGuildMembers",
                                         columns: table => new
                                                           {
                                                               GuildId = table.Column<long>(type: "bigint", nullable: false),
                                                               Name = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                                                               Rank = table.Column<string>(type: "nvarchar(max)", nullable: true)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildWarsGuildMembers",
                                                                           x => new
                                                                                {
                                                                                    x.GuildId,
                                                                                    x.Name
                                                                                });

                                                          table.ForeignKey(name: "FK_GuildWarsGuildMembers_Guilds_GuildId",
                                                                           column: x => x.GuildId,
                                                                           principalTable: "Guilds",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
        }
    }
}