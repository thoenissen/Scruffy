using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 70
    /// </summary>
    public partial class Update70 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiscordServerMembers",
                columns: table => new
                {
                    ServerId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    AccountId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordServerMembers", x => new { x.ServerId, x.AccountId });
                    table.ForeignKey(
                        name: "FK_DiscordServerMembers_DiscordAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "DiscordAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscordServerMembers_AccountId",
                table: "DiscordServerMembers",
                column: "AccountId");
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscordServerMembers");
        }
    }
}