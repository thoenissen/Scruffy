using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 69
    /// </summary>
    public partial class Update69 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(name: "Group",
                                            table: "RaidRegistrations",
                                            type: "int",
                                            nullable: false,
                                            defaultValue: 0);
            migrationBuilder.AddColumn<long>(name: "LineUpRoleId", table: "RaidRegistrations", type: "bigint", nullable: true);
            migrationBuilder.CreateIndex(name: "IX_RaidRegistrations_LineUpRoleId", table: "RaidRegistrations", column: "LineUpRoleId");

            migrationBuilder.AddForeignKey(name: "FK_RaidRegistrations_RaidRoles_LineUpRoleId",
                                           table: "RaidRegistrations",
                                           column: "LineUpRoleId",
                                           principalTable: "RaidRoles",
                                           principalColumn: "Id");
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_RaidRegistrations_RaidRoles_LineUpRoleId", table: "RaidRegistrations");
            migrationBuilder.DropIndex(name: "IX_RaidRegistrations_LineUpRoleId", table: "RaidRegistrations");
            migrationBuilder.DropColumn(name: "Group", table: "RaidRegistrations");
            migrationBuilder.DropColumn(name: "LineUpRoleId", table: "RaidRegistrations");
        }
    }
}