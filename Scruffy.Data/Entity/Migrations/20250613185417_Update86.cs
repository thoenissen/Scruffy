using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scruffy.Data.Entity.Migrations;

/// <inheritdoc />
public partial class Update86 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>("PrivilegedMemberRoleId", table: "Guilds", type: "decimal(20,0)", nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("PrivilegedMemberRoleId", "Guilds");
    }
}