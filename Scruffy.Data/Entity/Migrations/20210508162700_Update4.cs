using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 4
    /// </summary>
    public partial class Update4 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>("AppointmentId", "FractalRegistrations", "bigint", nullable: true);

            migrationBuilder.CreateTable("FractalAppointments",
                                         table => new
                                                  {
                                                      Id = table.Column<long>("bigint", nullable: false)
                                                                .Annotation("SqlServer:Identity", "1, 1"),
                                                      ConfigurationId = table.Column<long>("bigint", nullable: false),
                                                      MessageId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      AppointmentTimeStamp = table.Column<DateTime>("datetime2", nullable: false)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_FractalAppointments", x => x.Id);

                                                          table.ForeignKey("FK_FractalAppointments_FractalLfgConfigurations_ConfigurationId",
                                                                           x => x.ConfigurationId,
                                                                           "FractalLfgConfigurations",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
            migrationBuilder.CreateIndex("IX_FractalRegistrations_AppointmentId", "FractalRegistrations", "AppointmentId");
            migrationBuilder.CreateIndex("IX_FractalAppointments_ConfigurationId", "FractalAppointments", "ConfigurationId");

            migrationBuilder.AddForeignKey("FK_FractalRegistrations_FractalAppointments_AppointmentId",
                                           "FractalRegistrations",
                                           "AppointmentId",
                                           "FractalAppointments",
                                           principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_FractalRegistrations_FractalAppointments_AppointmentId", "FractalRegistrations");
            migrationBuilder.DropTable("FractalAppointments");
            migrationBuilder.DropIndex("IX_FractalRegistrations_AppointmentId", "FractalRegistrations");
            migrationBuilder.DropColumn("AppointmentId", "FractalRegistrations");
        }
    }
}