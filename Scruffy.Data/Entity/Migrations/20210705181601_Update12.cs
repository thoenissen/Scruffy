using System;

using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 12
    /// </summary>
    public partial class Update12 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("CalendarAppointmentTemplates",
                                         table => new
                                                  {
                                                      Id = table.Column<long>("bigint", nullable: false)
                                                                .Annotation("SqlServer:Identity", "1, 1"),
                                                      Description = table.Column<string>("nvarchar(max)", nullable: true),
                                                      ReminderMessage = table.Column<string>("nvarchar(max)", nullable: true)
                                                  },
                                         constraints: table => table.PrimaryKey("PK_CalendarAppointmentTemplates", x => x.Id));

            migrationBuilder.CreateTable("CalendarAppointments",
                                         table => new
                                                  {
                                                      Id = table.Column<long>("bigint", nullable: false)
                                                                .Annotation("SqlServer:Identity", "1, 1"),
                                                      TimeStamp = table.Column<DateTime>("datetime2", nullable: false),
                                                      CalendarAppointmentTemplateId = table.Column<long>("bigint", nullable: false)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_CalendarAppointments", x => x.Id);

                                                          table.ForeignKey("FK_CalendarAppointments_CalendarAppointmentTemplates_CalendarAppointmentTemplateId",
                                                                           x => x.CalendarAppointmentTemplateId,
                                                                           "CalendarAppointmentTemplates",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable("CalendarAppointmentSchedules",
                                         table => new
                                                  {
                                                      Id = table.Column<long>("bigint", nullable: false)
                                                                .Annotation("SqlServer:Identity", "1, 1"),
                                                      CalendarAppointmentTemplateId = table.Column<long>("bigint", nullable: false),
                                                      Type = table.Column<int>("int", nullable: false),
                                                      AdditionalData = table.Column<string>("nvarchar(max)", nullable: true)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_CalendarAppointmentSchedules", x => x.Id);

                                                          table.ForeignKey("FK_CalendarAppointmentSchedules_CalendarAppointmentTemplates_CalendarAppointmentTemplateId",
                                                                           x => x.CalendarAppointmentTemplateId,
                                                                           "CalendarAppointmentTemplates",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
            migrationBuilder.CreateIndex("IX_CalendarAppointments_CalendarAppointmentTemplateId", "CalendarAppointments", "CalendarAppointmentTemplateId");
            migrationBuilder.CreateIndex("IX_CalendarAppointmentSchedules_CalendarAppointmentTemplateId", "CalendarAppointmentSchedules", "CalendarAppointmentTemplateId");
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("CalendarAppointments");
            migrationBuilder.DropTable("CalendarAppointmentSchedules");
            migrationBuilder.DropTable("CalendarAppointmentTemplates");
        }
    }
}