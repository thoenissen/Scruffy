#pragma warning disable RH0201

using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations;

/// <inheritdoc />
public partial class Update93 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_RaidRegistrations_RaidRoles_LineUpRoleId", table: "RaidRegistrations");
        migrationBuilder.DropIndex(name: "IX_RaidRegistrations_LineUpRoleId", table: "RaidRegistrations");
        migrationBuilder.DropColumn(name: "Group", table: "RaidRegistrations");
        migrationBuilder.DropColumn(name: "LineUpRoleId", table: "RaidRegistrations");
        migrationBuilder.AddColumn<bool>(name: "IsRoleWishFulfilled", table: "RaidRegistrations", type: "bit", nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "IsRoleWishFulfilled", table: "RaidRegistrations");
        migrationBuilder.AddColumn<int>(name: "Group", table: "RaidRegistrations", type: "int", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<long>(name: "LineUpRoleId", table: "RaidRegistrations", type: "bigint", nullable: true);
        migrationBuilder.CreateIndex(name: "IX_RaidRegistrations_LineUpRoleId", table: "RaidRegistrations", column: "LineUpRoleId");
        migrationBuilder.AddForeignKey(name: "FK_RaidRegistrations_RaidRoles_LineUpRoleId", table: "RaidRegistrations", column: "LineUpRoleId", principalTable: "RaidRoles", principalColumn: "Id");
    }
}