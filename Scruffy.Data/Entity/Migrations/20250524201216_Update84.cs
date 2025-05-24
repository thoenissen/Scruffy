using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations;

/// <inheritdoc />
public partial class Update84 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(name: "ThreadId",
                                              table: "LookingForGroupAppointments",
                                              type: "decimal(20,0)",
                                              nullable: true,
                                              oldClrType: typeof(decimal),
                                              oldType: "decimal(20,0)");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(name: "ThreadId",
                                              table: "LookingForGroupAppointments",
                                              type: "decimal(20,0)",
                                              nullable: false,
                                              defaultValue: 0m,
                                              oldClrType: typeof(decimal),
                                              oldType: "decimal(20,0)",
                                              oldNullable: true);
    }
}