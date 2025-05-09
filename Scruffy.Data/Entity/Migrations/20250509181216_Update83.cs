using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <inheritdoc />
    public partial class Update83 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>("ParticipantCount", "LookingForGroupAppointments", "int", nullable: true, oldClrType: typeof(decimal), oldType: "decimal(20,0)", oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>("ParticipantCount", "LookingForGroupAppointments", "decimal(20,0)", nullable: true, oldClrType: typeof(int), oldType: "int", oldNullable: true);
        }
    }
}