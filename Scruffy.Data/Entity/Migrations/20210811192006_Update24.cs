
using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 24
    /// </summary>
    public partial class Update24 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("GuildSpecialRankConfigurations",
                                         table => new
                                                  {
                                                      Id = table.Column<long>("bigint", nullable: false)
                                                                .Annotation("SqlServer:Identity", "1, 1"),
                                                      GuildId = table.Column<long>("bigint", nullable: false),
                                                      Description = table.Column<string>("nvarchar(max)", nullable: true),
                                                      DiscordRoleId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      MaximumPoints = table.Column<double>("float", nullable: false),
                                                      GrantThreshold = table.Column<double>("float", nullable: false),
                                                      RemoveThreshold = table.Column<double>("float", nullable: false),
                                                      IsDeleted = table.Column<bool>("bit", nullable: false)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildSpecialRankConfigurations", x => x.Id);

                                                          table.ForeignKey("FK_GuildSpecialRankConfigurations_Guilds_GuildId",
                                                                           x => x.GuildId,
                                                                           "Guilds",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable("GuildSpecialRankPoints",
                                         table => new
                                                  {
                                                      ConfigurationId = table.Column<long>("bigint", nullable: false),
                                                      UserId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      Points = table.Column<double>("float", nullable: false)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildSpecialRankPoints",
                                                                           x => new
                                                                                {
                                                                                    x.ConfigurationId,
                                                                                    x.UserId
                                                                                });

                                                          table.ForeignKey("FK_GuildSpecialRankPoints_GuildSpecialRankConfigurations_ConfigurationId",
                                                                           x => x.ConfigurationId,
                                                                           "GuildSpecialRankConfigurations",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey("FK_GuildSpecialRankPoints_Users_UserId",
                                                                           x => x.UserId,
                                                                           "Users",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable("GuildSpecialRankProtocolEntries",
                                         table => new
                                                  {
                                                      Id = table.Column<long>("bigint", nullable: false)
                                                                .Annotation("SqlServer:Identity", "1, 1"),
                                                      TimeStamp = table.Column<DateTime>("datetime2", nullable: false),
                                                      ConfigurationId = table.Column<long>("bigint", nullable: false),
                                                      Type = table.Column<int>("int", nullable: false),
                                                      UserId = table.Column<decimal>("decimal(20,0)", nullable: true),
                                                      Amount = table.Column<decimal>("decimal(20,0)", nullable: true)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildSpecialRankProtocolEntries", x => x.Id);

                                                          table.ForeignKey("FK_GuildSpecialRankProtocolEntries_GuildSpecialRankConfigurations_ConfigurationId",
                                                                           x => x.ConfigurationId,
                                                                           "GuildSpecialRankConfigurations",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey("FK_GuildSpecialRankProtocolEntries_Users_UserId",
                                                                           x => x.UserId,
                                                                           "Users",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable("GuildSpecialRankRoleAssignments",
                                         table => new
                                                  {
                                                      ConfigurationId = table.Column<long>("bigint", nullable: false),
                                                      DiscordRoleId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      Points = table.Column<double>("float", nullable: false)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildSpecialRankRoleAssignments",
                                                                           x => new
                                                                                {
                                                                                    x.ConfigurationId,
                                                                                    x.DiscordRoleId
                                                                                });

                                                          table.ForeignKey("FK_GuildSpecialRankRoleAssignments_GuildSpecialRankConfigurations_ConfigurationId",
                                                                           x => x.ConfigurationId,
                                                                           "GuildSpecialRankConfigurations",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
            migrationBuilder.CreateIndex("IX_GuildSpecialRankConfigurations_GuildId", "GuildSpecialRankConfigurations", "GuildId");
            migrationBuilder.CreateIndex("IX_GuildSpecialRankPoints_UserId", "GuildSpecialRankPoints", "UserId");
            migrationBuilder.CreateIndex("IX_GuildSpecialRankProtocolEntries_ConfigurationId", "GuildSpecialRankProtocolEntries", "ConfigurationId");
            migrationBuilder.CreateIndex("IX_GuildSpecialRankProtocolEntries_UserId", "GuildSpecialRankProtocolEntries", "UserId");
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("GuildSpecialRankPoints");
            migrationBuilder.DropTable("GuildSpecialRankProtocolEntries");
            migrationBuilder.DropTable("GuildSpecialRankRoleAssignments");
            migrationBuilder.DropTable("GuildSpecialRankConfigurations");
        }
    }
}