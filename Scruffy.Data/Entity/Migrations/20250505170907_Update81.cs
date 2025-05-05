using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <inheritdoc />
    public partial class Update81 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>("Remarks",
                                               table: "RaidAppointmentLineUpSquads",
                                               type: "nvarchar(max)",
                                               nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("Remarks", "RaidAppointmentLineUpSquads");
        }
    }
}