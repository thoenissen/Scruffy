using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 72
    /// </summary>
    public partial class Update72 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(name: "RaidSpecialRoles",
                                         columns: table => new
                                                           {
                                                               Id = table.Column<long>(type: "bigint", nullable: false)
                                                                         .Annotation("SqlServer:Identity", "1, 1"),
                                                               Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                               DiscordEmojiId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                                                           },
                                         constraints: table => table.PrimaryKey("PK_RaidSpecialRoles", x => x.Id));

            migrationBuilder.CreateTable(name: "RaidUserSpecialRoles",
                                         columns: table => new
                                                           {
                                                               UserId = table.Column<long>(type: "bigint", nullable: false),
                                                               SpecialRoleId = table.Column<long>(type: "bigint", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_RaidUserSpecialRoles",
                                                                           x => new
                                                                                {
                                                                                    x.UserId,
                                                                                    x.SpecialRoleId
                                                                                });

                                                          table.ForeignKey(name: "FK_RaidUserSpecialRoles_RaidSpecialRoles_SpecialRoleId",
                                                                           column: x => x.SpecialRoleId,
                                                                           principalTable: "RaidSpecialRoles",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey(name: "FK_RaidUserSpecialRoles_Users_UserId",
                                                                           column: x => x.UserId,
                                                                           principalTable: "Users",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
            migrationBuilder.CreateIndex(name: "IX_RaidUserSpecialRoles_SpecialRoleId", table: "RaidUserSpecialRoles", column: "SpecialRoleId");
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "RaidUserSpecialRoles");
            migrationBuilder.DropTable(name: "RaidSpecialRoles");
        }
    }
}