using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 27
    /// </summary>
    public partial class Update27 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("GameChannels",
                                         table => new
                                                  {
                                                      Id = table.Column<long>("bigint", nullable: false)
                                                                .Annotation("SqlServer:Identity", "1, 1"),
                                                      Type = table.Column<int>("int", nullable: false),
                                                      DiscordChannelId = table.Column<decimal>("decimal(20,0)", nullable: false)
                                                  },
                                         constraints: table => table.PrimaryKey("PK_GameChannels", x => x.Id));
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("GameChannels");
        }
    }
}