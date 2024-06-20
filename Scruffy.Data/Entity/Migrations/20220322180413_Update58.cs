using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 58
    /// </summary>
    public partial class Update58 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(name: "GuildUserConfigurations",
                                         columns: table => new
                                                           {
                                                               GuildId = table.Column<long>(type: "bigint", nullable: false),
                                                               UserId = table.Column<long>(type: "bigint", nullable: false),
                                                               IsFixedRank = table.Column<bool>(type: "bit", nullable: false),
                                                               IsInactive = table.Column<bool>(type: "bit", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildUserConfigurations",
                                                                           x => new
                                                                                {
                                                                                    x.GuildId,
                                                                                    x.UserId
                                                                                });

                                                          table.ForeignKey(name: "FK_GuildUserConfigurations_Guilds_GuildId",
                                                                           column: x => x.GuildId,
                                                                           principalTable: "Guilds",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GuildUserConfigurations");
        }
    }
}