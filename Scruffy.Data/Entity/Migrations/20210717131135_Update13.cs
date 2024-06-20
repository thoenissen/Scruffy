using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 13
    /// </summary>
    public partial class Update13 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>("RaidExperienceLevelId", "Users", "bigint", nullable: true);

            migrationBuilder.AddColumn<DateTime>("Deadline",
                                                 "RaidAppointments",
                                                 "datetime2",
                                                 nullable: false,
                                                 defaultValue: new DateTime(1,
                                                                            1,
                                                                            1,
                                                                            0,
                                                                            0,
                                                                            0,
                                                                            0,
                                                                            DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>("TemplateId",
                                             "RaidAppointments",
                                             "bigint",
                                             nullable: false,
                                             defaultValue: 0L);
            migrationBuilder.CreateIndex("IX_Users_RaidExperienceLevelId", "Users", "RaidExperienceLevelId");
            migrationBuilder.CreateIndex("IX_RaidAppointments_TemplateId", "RaidAppointments", "TemplateId");

            migrationBuilder.AddForeignKey("FK_RaidAppointments_RaidDayTemplates_TemplateId",
                                           "RaidAppointments",
                                           "TemplateId",
                                           "RaidDayTemplates",
                                           principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Users_RaidExperienceLevels_RaidExperienceLevelId",
                                           "Users",
                                           "RaidExperienceLevelId",
                                           "RaidExperienceLevels",
                                           principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_RaidAppointments_RaidDayTemplates_TemplateId", "RaidAppointments");
            migrationBuilder.DropForeignKey("FK_Users_RaidExperienceLevels_RaidExperienceLevelId", "Users");
            migrationBuilder.DropIndex("IX_Users_RaidExperienceLevelId", "Users");
            migrationBuilder.DropIndex("IX_RaidAppointments_TemplateId", "RaidAppointments");
            migrationBuilder.DropColumn("RaidExperienceLevelId", "Users");
            migrationBuilder.DropColumn("Deadline", "RaidAppointments");
            migrationBuilder.DropColumn("TemplateId", "RaidAppointments");
        }
    }
}