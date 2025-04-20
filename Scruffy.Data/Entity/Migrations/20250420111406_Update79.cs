using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations;

/// <inheritdoc />
public partial class Update79 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>("HealerRaidRole", "RaidAppointmentLineUpSquads", "int", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<int>("TankRaidRole", "RaidAppointmentLineUpSquads", "int", nullable: false, defaultValue: 0);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("HealerRaidRole", "RaidAppointmentLineUpSquads");
        migrationBuilder.DropColumn("TankRaidRole", "RaidAppointmentLineUpSquads");
    }
}