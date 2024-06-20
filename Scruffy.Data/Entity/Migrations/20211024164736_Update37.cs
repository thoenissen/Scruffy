using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 37
    /// </summary>
    public partial class Update37 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(name: "Permissions",
                                                table: "GuildWarsAccounts",
                                                type: "decimal(20,0)",
                                                nullable: false,
                                                defaultValue: 0m);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Permissions", table: "GuildWarsAccounts");
        }
    }
}