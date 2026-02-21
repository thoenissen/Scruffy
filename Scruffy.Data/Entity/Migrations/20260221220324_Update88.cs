using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations;

/// <inheritdoc />
public partial class Update88 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "DiscordMessageExperienceLogEntries",
                                     columns: table => new
                                     {
                                         ServerId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                         MessageId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                         DiscordAccountId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                         Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                                         ExperiencePoints = table.Column<int>(type: "int", nullable: false)
                                     },
                                     constraints: table => table.PrimaryKey("PK_DiscordMessageExperienceLogEntries", x => new { x.ServerId, x.MessageId }));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "DiscordMessageExperienceLogEntries");
    }
}