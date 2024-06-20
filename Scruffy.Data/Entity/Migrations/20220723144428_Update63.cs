using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 63
    /// </summary>
    public partial class Update63 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<decimal>(name: "NewUserDiscordRoleId", table: "Guilds", type: "decimal(20,0)", nullable: true);

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(name: "NewUserDiscordRoleId", table: "Guilds");
    }
}