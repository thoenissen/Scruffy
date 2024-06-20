using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 26
    /// </summary>
    public partial class Update26 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("GuildSpecialRankIgnoreRoleAssignments",
                                         table => new
                                                  {
                                                      ConfigurationId = table.Column<long>("bigint", nullable: false),
                                                      DiscordRoleId = table.Column<decimal>("decimal(20,0)", nullable: false)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildSpecialRankIgnoreRoleAssignments",
                                                                           x => new
                                                                                {
                                                                                    x.ConfigurationId,
                                                                                    x.DiscordRoleId
                                                                                });

                                                          table.ForeignKey("FK_GuildSpecialRankIgnoreRoleAssignments_GuildSpecialRankConfigurations_ConfigurationId",
                                                                           x => x.ConfigurationId,
                                                                           "GuildSpecialRankConfigurations",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("GuildSpecialRankIgnoreRoleAssignments");
        }
    }
}