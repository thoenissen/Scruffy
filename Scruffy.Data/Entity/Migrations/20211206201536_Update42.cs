using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 42
    /// </summary>
    public partial class Update42 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(name: "GroupCount",
                                            table: "RaidAppointments",
                                            type: "int",
                                            nullable: false,
                                            defaultValue: 0);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "GroupCount", table: "RaidAppointments");
        }
    }
}