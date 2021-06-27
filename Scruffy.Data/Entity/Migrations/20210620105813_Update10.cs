using System;

using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 10
    /// </summary>
    public partial class Update10 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_RaidExperienceAssignments_RaidDayConfigurations_ConfigurationId", "RaidExperienceAssignments");
            migrationBuilder.DropForeignKey("FK_RaidRequiredRoles_RaidDayConfigurations_ConfigurationId", "RaidRequiredRoles");
            migrationBuilder.DropColumn("AdministrationRoleId", "RaidDayConfigurations");
            migrationBuilder.DropColumn("ReminderChannelId", "RaidDayConfigurations");
            migrationBuilder.DropColumn("ReminderTime", "RaidDayConfigurations");
            migrationBuilder.DropColumn("ResetTime", "RaidDayConfigurations");
            migrationBuilder.RenameColumn("ConfigurationId", "RaidRequiredRoles", "TemplateId");
            migrationBuilder.RenameColumn("ConfigurationId", "RaidExperienceAssignments", "TemplateId");

            migrationBuilder.AlterColumn<string>("AliasName",
                                                 "RaidDayTemplates",
                                                 "nvarchar(20)",
                                                 maxLength: 20,
                                                 nullable: true,
                                                 oldClrType: typeof(string),
                                                 oldType: "nvarchar(max)",
                                                 oldNullable: true);

            migrationBuilder.AddColumn<string>("AliasName",
                                               "RaidDayConfigurations",
                                               "nvarchar(20)",
                                               maxLength: 20,
                                               nullable: true);

            migrationBuilder.AddForeignKey("FK_RaidExperienceAssignments_RaidDayTemplates_TemplateId",
                                           "RaidExperienceAssignments",
                                           "TemplateId",
                                           "RaidDayTemplates",
                                           principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_RaidRequiredRoles_RaidDayTemplates_TemplateId",
                                           "RaidRequiredRoles",
                                           "TemplateId",
                                           "RaidDayTemplates",
                                           principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_RaidExperienceAssignments_RaidDayTemplates_TemplateId", "RaidExperienceAssignments");
            migrationBuilder.DropForeignKey("FK_RaidRequiredRoles_RaidDayTemplates_TemplateId", "RaidRequiredRoles");
            migrationBuilder.DropColumn("AliasName", "RaidDayConfigurations");
            migrationBuilder.RenameColumn("TemplateId", "RaidRequiredRoles", "ConfigurationId");
            migrationBuilder.RenameColumn("TemplateId", "RaidExperienceAssignments", "ConfigurationId");

            migrationBuilder.AlterColumn<string>("AliasName",
                                                 "RaidDayTemplates",
                                                 "nvarchar(max)",
                                                 nullable: true,
                                                 oldClrType: typeof(string),
                                                 oldType: "nvarchar(20)",
                                                 oldMaxLength: 20,
                                                 oldNullable: true);
            migrationBuilder.AddColumn<decimal>("AdministrationRoleId", "RaidDayConfigurations", "decimal(20,0)", nullable: true);
            migrationBuilder.AddColumn<decimal>("ReminderChannelId", "RaidDayConfigurations", "decimal(20,0)", nullable: true);
            migrationBuilder.AddColumn<TimeSpan>("ReminderTime", "RaidDayConfigurations", "time", nullable: true);

            migrationBuilder.AddColumn<TimeSpan>("ResetTime",
                                                 "RaidDayConfigurations",
                                                 "time",
                                                 nullable: false,
                                                 defaultValue: new TimeSpan(0,
                                                                            0,
                                                                            0,
                                                                            0,
                                                                            0));

            migrationBuilder.AddForeignKey("FK_RaidExperienceAssignments_RaidDayConfigurations_ConfigurationId",
                                           "RaidExperienceAssignments",
                                           "ConfigurationId",
                                           "RaidDayConfigurations",
                                           principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_RaidRequiredRoles_RaidDayConfigurations_ConfigurationId",
                                           "RaidRequiredRoles",
                                           "ConfigurationId",
                                           "RaidDayConfigurations",
                                           principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);
        }
    }
}