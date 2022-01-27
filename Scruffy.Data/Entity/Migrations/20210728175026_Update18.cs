
using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 18
    /// </summary>
    public partial class Update18 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>("AppointmentTime", "CalendarAppointmentTemplates", "time", nullable: true);
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("AppointmentTime", "CalendarAppointmentTemplates");
        }
    }
}