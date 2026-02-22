using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations;

/// <inheritdoc />
public partial class Update88 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "IsBatchCommitted", table: "DiscordMessages");
        migrationBuilder.AddColumn<decimal>(name: "DiscordThreadId",
                                            table: "DiscordMessages",
                                            type: "decimal(20,0)",
                                            nullable: false,
                                            defaultValue: 0m);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "DiscordThreadId", table: "DiscordMessages");
        migrationBuilder.AddColumn<bool>(name: "IsBatchCommitted",
                                         table: "DiscordMessages",
                                         type: "bit",
                                         nullable: false,
                                         defaultValue: false);
    }
}