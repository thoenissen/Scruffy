#pragma warning disable RH0201

using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations;

/// <inheritdoc />
public partial class Update89 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "DiscordServerChannels",
                                     columns: table => new
                                     {
                                         ServerId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                         ChannelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                         Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                                     },
                                     constraints: table => table.PrimaryKey("PK_DiscordServerChannels", x => new { x.ServerId, x.ChannelId }));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "DiscordServerChannels");
    }
}