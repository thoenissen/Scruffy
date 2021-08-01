using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 20
    /// </summary>
    public partial class Update20 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>("MessageOfTheDayChannelId", "Guilds", "decimal(20,0)", nullable: true);
            migrationBuilder.AddColumn<decimal>("MessageOfTheDayMessageId", "Guilds", "decimal(20,0)", nullable: true);

            migrationBuilder.AlterColumn<long>("CalendarAppointmentScheduleId",
                                               "CalendarAppointments",
                                               "bigint",
                                               nullable: true,
                                               oldClrType: typeof(long),
                                               oldType: "bigint");
            migrationBuilder.AddColumn<decimal>("ReminderChannelId", "CalendarAppointments", "decimal(20,0)", nullable: true);
            migrationBuilder.AddColumn<decimal>("ReminderMessageId", "CalendarAppointments", "decimal(20,0)", nullable: true);
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("MessageOfTheDayChannelId", "Guilds");
            migrationBuilder.DropColumn("MessageOfTheDayMessageId", "Guilds");
            migrationBuilder.DropColumn("ReminderChannelId", "CalendarAppointments");
            migrationBuilder.DropColumn("ReminderMessageId", "CalendarAppointments");

            migrationBuilder.AlterColumn<long>("CalendarAppointmentScheduleId",
                                               "CalendarAppointments",
                                               "bigint",
                                               nullable: false,
                                               defaultValue: 0L,
                                               oldClrType: typeof(long),
                                               oldType: "bigint",
                                               oldNullable: true);
        }
    }
}