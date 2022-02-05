using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 16
    /// </summary>
    public partial class Update16 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_Guilds_ServerConfigurationEntity_DiscordServerId", "Guilds");
            migrationBuilder.DropPrimaryKey("PK_ServerConfigurationEntity", "ServerConfigurationEntity");
            migrationBuilder.RenameTable("ServerConfigurationEntity", newName: "ServerConfigurations");
            migrationBuilder.AddColumn<int>("GuildPoints", "CalendarAppointmentTemplates", "int", nullable: true);

            migrationBuilder.AddColumn<bool>("IsDeleted",
                                             "CalendarAppointmentTemplates",
                                             "bit",
                                             nullable: false,
                                             defaultValue: false);
            migrationBuilder.AddColumn<bool>("IsRaisingGuildPointCap", "CalendarAppointmentTemplates", "bit", nullable: true);
            migrationBuilder.AddColumn<TimeSpan>("ReminderTime", "CalendarAppointmentTemplates", "time", nullable: true);

            migrationBuilder.AddColumn<decimal>("ServerId",
                                                "CalendarAppointmentTemplates",
                                                "decimal(20,0)",
                                                nullable: false,
                                                defaultValue: 0m);
            migrationBuilder.AddColumn<string>("Uri", "CalendarAppointmentTemplates", "nvarchar(max)", nullable: true);

            migrationBuilder.AddColumn<long>("ServerId",
                                             "CalendarAppointmentSchedules",
                                             "bigint",
                                             nullable: false,
                                             defaultValue: 0L);
            migrationBuilder.AddPrimaryKey("PK_ServerConfigurations", "ServerConfigurations", "ServerId");
            migrationBuilder.CreateIndex("IX_CalendarAppointmentTemplates_ServerId", "CalendarAppointmentTemplates", "ServerId");

            migrationBuilder.AddForeignKey("FK_CalendarAppointmentTemplates_ServerConfigurations_ServerId",
                                           "CalendarAppointmentTemplates",
                                           "ServerId",
                                           "ServerConfigurations",
                                           principalColumn: "ServerId",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Guilds_ServerConfigurations_DiscordServerId",
                                           "Guilds",
                                           "DiscordServerId",
                                           "ServerConfigurations",
                                           principalColumn: "ServerId",
                                           onDelete: ReferentialAction.Restrict);
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_CalendarAppointmentTemplates_ServerConfigurations_ServerId", "CalendarAppointmentTemplates");
            migrationBuilder.DropForeignKey("FK_Guilds_ServerConfigurations_DiscordServerId", "Guilds");
            migrationBuilder.DropIndex("IX_CalendarAppointmentTemplates_ServerId", "CalendarAppointmentTemplates");
            migrationBuilder.DropPrimaryKey("PK_ServerConfigurations", "ServerConfigurations");
            migrationBuilder.DropColumn("GuildPoints", "CalendarAppointmentTemplates");
            migrationBuilder.DropColumn("IsDeleted", "CalendarAppointmentTemplates");
            migrationBuilder.DropColumn("IsRaisingGuildPointCap", "CalendarAppointmentTemplates");
            migrationBuilder.DropColumn("ReminderTime", "CalendarAppointmentTemplates");
            migrationBuilder.DropColumn("ServerId", "CalendarAppointmentTemplates");
            migrationBuilder.DropColumn("Uri", "CalendarAppointmentTemplates");
            migrationBuilder.DropColumn("ServerId", "CalendarAppointmentSchedules");
            migrationBuilder.RenameTable("ServerConfigurations", newName: "ServerConfigurationEntity");
            migrationBuilder.AddPrimaryKey("PK_ServerConfigurationEntity", "ServerConfigurationEntity", "ServerId");

            migrationBuilder.AddForeignKey("FK_Guilds_ServerConfigurationEntity_DiscordServerId",
                                           "Guilds",
                                           "DiscordServerId",
                                           "ServerConfigurationEntity",
                                           principalColumn: "ServerId",
                                           onDelete: ReferentialAction.Restrict);
        }
    }
}