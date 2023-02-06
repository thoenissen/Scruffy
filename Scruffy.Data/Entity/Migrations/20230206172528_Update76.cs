using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations;

/// <summary>
/// Update 79
/// </summary>
public partial class Update76 : Migration
{
    /// <summary>
    /// Upgrade
    /// </summary>
    /// <param name="migrationBuilder">Builder</param>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(name: "LeaderId",
                                         table: "CalendarAppointments",
                                         type: "bigint",
                                         nullable: true);

        migrationBuilder.CreateIndex(name: "IX_CalendarAppointments_LeaderId",
                                     table: "CalendarAppointments",
                                     column: "LeaderId");

        migrationBuilder.AddForeignKey(name: "FK_CalendarAppointments_Users_LeaderId",
                                       table: "CalendarAppointments",
                                       column: "LeaderId",
                                       principalTable: "Users",
                                       principalColumn: "Id");
    }

    /// <summary>
    /// Downgrade
    /// </summary>
    /// <param name="migrationBuilder">Builder</param>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_CalendarAppointments_Users_LeaderId",
                                        table: "CalendarAppointments");

        migrationBuilder.DropIndex(name: "IX_CalendarAppointments_LeaderId",
                                   table: "CalendarAppointments");

        migrationBuilder.DropColumn(name: "LeaderId",
                                    table: "CalendarAppointments");
    }
}