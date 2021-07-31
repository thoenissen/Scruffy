using System;

using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 19
    /// </summary>
    public partial class Update19 : Migration
    {
        /// <summary>
        /// /Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>("AdministratorRoleId", "ServerConfigurations", "decimal(20,0)", nullable: true);

            migrationBuilder.AlterColumn<decimal>("GuildCalendarMessageId",
                                                  "Guilds",
                                                  "decimal(20,0)",
                                                  nullable: true,
                                                  oldClrType: typeof(long),
                                                  oldType: "bigint",
                                                  oldNullable: true);

            migrationBuilder.AlterColumn<decimal>("GuildCalendarChannelId",
                                                  "Guilds",
                                                  "decimal(20,0)",
                                                  nullable: true,
                                                  oldClrType: typeof(long),
                                                  oldType: "bigint",
                                                  oldNullable: true);

            migrationBuilder.AlterColumn<TimeSpan>("AppointmentTime",
                                                   "CalendarAppointmentTemplates",
                                                   "time",
                                                   nullable: false,
                                                   defaultValue: new TimeSpan(0,
                                                                              0,
                                                                              0,
                                                                              0,
                                                                              0),
                                                   oldClrType: typeof(TimeSpan),
                                                   oldType: "time",
                                                   oldNullable: true);

            migrationBuilder.AddColumn<bool>("IsDeleted",
                                             "CalendarAppointmentSchedules",
                                             "bit",
                                             nullable: false,
                                             defaultValue: false);

            migrationBuilder.AddColumn<long>("CalendarAppointmentScheduleId",
                                             "CalendarAppointments",
                                             "bigint",
                                             nullable: false,
                                             defaultValue: 0L);

            migrationBuilder.CreateTable("CalendarAppointmentParticipants",
                                         table => new
                                                  {
                                                      AppointmentId = table.Column<long>("bigint", nullable: false),
                                                      UserId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      IsLeader = table.Column<bool>("bit", nullable: false)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_CalendarAppointmentParticipants",
                                                                           x => new
                                                                                {
                                                                                    x.AppointmentId,
                                                                                    x.UserId
                                                                                });

                                                          table.ForeignKey("FK_CalendarAppointmentParticipants_CalendarAppointments_AppointmentId",
                                                                           x => x.AppointmentId,
                                                                           "CalendarAppointments",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey("FK_CalendarAppointmentParticipants_Users_UserId",
                                                                           x => x.UserId,
                                                                           "Users",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
            migrationBuilder.CreateIndex("IX_CalendarAppointments_CalendarAppointmentScheduleId", "CalendarAppointments", "CalendarAppointmentScheduleId");
            migrationBuilder.CreateIndex("IX_CalendarAppointmentParticipants_UserId", "CalendarAppointmentParticipants", "UserId");

            migrationBuilder.AddForeignKey("FK_CalendarAppointments_CalendarAppointmentSchedules_CalendarAppointmentScheduleId",
                                           "CalendarAppointments",
                                           "CalendarAppointmentScheduleId",
                                           "CalendarAppointmentSchedules",
                                           principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_CalendarAppointments_CalendarAppointmentSchedules_CalendarAppointmentScheduleId", "CalendarAppointments");
            migrationBuilder.DropTable("CalendarAppointmentParticipants");
            migrationBuilder.DropIndex("IX_CalendarAppointments_CalendarAppointmentScheduleId", "CalendarAppointments");
            migrationBuilder.DropColumn("AdministratorRoleId", "ServerConfigurations");
            migrationBuilder.DropColumn("IsDeleted", "CalendarAppointmentSchedules");
            migrationBuilder.DropColumn("CalendarAppointmentScheduleId", "CalendarAppointments");

            migrationBuilder.AlterColumn<long>("GuildCalendarMessageId",
                                               "Guilds",
                                               "bigint",
                                               nullable: true,
                                               oldClrType: typeof(decimal),
                                               oldType: "decimal(20,0)",
                                               oldNullable: true);

            migrationBuilder.AlterColumn<long>("GuildCalendarChannelId",
                                               "Guilds",
                                               "bigint",
                                               nullable: true,
                                               oldClrType: typeof(decimal),
                                               oldType: "decimal(20,0)",
                                               oldNullable: true);

            migrationBuilder.AlterColumn<TimeSpan>("AppointmentTime",
                                                   "CalendarAppointmentTemplates",
                                                   "time",
                                                   nullable: true,
                                                   oldClrType: typeof(TimeSpan),
                                                   oldType: "time");
        }
    }
}