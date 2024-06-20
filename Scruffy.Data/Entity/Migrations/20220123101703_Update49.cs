using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 49
    /// </summary>
    public partial class Update49 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(name: "BlockedDiscordChannels",
                                         columns: table => new
                                                           {
                                                               ServerId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               ChannelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_BlockedDiscordChannels",
                                                                           x => new
                                                                                {
                                                                                    x.ServerId,
                                                                                    x.ChannelId
                                                                                });
                                                      });
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "BlockedDiscordChannels");
        }
    }
}