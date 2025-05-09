using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <inheritdoc />
    public partial class Update82 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>("Date", "LookingForGroupAppointments", "datetime2", nullable: true);
            migrationBuilder.AddColumn<decimal>("ParticipantCount", "LookingForGroupAppointments", "decimal(20,0)", nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("Date", "LookingForGroupAppointments");
            migrationBuilder.DropColumn("ParticipantCount", "LookingForGroupAppointments");
        }
    }
}