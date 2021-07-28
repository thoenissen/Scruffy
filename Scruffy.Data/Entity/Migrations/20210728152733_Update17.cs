using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 17
    /// </summary>
    public partial class Update17 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>("CalendarDescription", "Guilds", "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<string>("CalendarTitle", "Guilds", "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<long>("GuildCalendarChannelId", "Guilds", "bigint", nullable: true);
            migrationBuilder.AddColumn<long>("GuildCalendarMessageId", "Guilds", "bigint", nullable: true);

            migrationBuilder.AlterColumn<decimal>("ServerId",
                                                  "CalendarAppointmentSchedules",
                                                  "decimal(20,0)",
                                                  nullable: false,
                                                  oldClrType: typeof(long),
                                                  oldType: "bigint");
            migrationBuilder.AddColumn<string>("Description", "CalendarAppointmentSchedules", "nvarchar(max)", nullable: true);
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("CalendarDescription", "Guilds");
            migrationBuilder.DropColumn("CalendarTitle", "Guilds");
            migrationBuilder.DropColumn("GuildCalendarChannelId", "Guilds");
            migrationBuilder.DropColumn("GuildCalendarMessageId", "Guilds");
            migrationBuilder.DropColumn("Description", "CalendarAppointmentSchedules");

            migrationBuilder.AlterColumn<long>("ServerId",
                                               "CalendarAppointmentSchedules",
                                               "bigint",
                                               nullable: false,
                                               oldClrType: typeof(decimal),
                                               oldType: "decimal(20,0)");
        }
    }
}