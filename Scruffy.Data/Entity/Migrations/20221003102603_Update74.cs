using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 74
    /// </summary>
    public partial class Update74 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "DiscordEventDescription", table: "CalendarAppointmentTemplates", type: "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<decimal>(name: "DiscordVoiceChannel", table: "CalendarAppointmentTemplates", type: "decimal(20,0)", nullable: true);
            migrationBuilder.AddColumn<decimal>(name: "DiscordEventId", table: "CalendarAppointments", type: "decimal(20,0)", nullable: true);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DiscordEventDescription", table: "CalendarAppointmentTemplates");
            migrationBuilder.DropColumn(name: "DiscordVoiceChannel", table: "CalendarAppointmentTemplates");
            migrationBuilder.DropColumn(name: "DiscordEventId", table: "CalendarAppointments");
        }
    }
}