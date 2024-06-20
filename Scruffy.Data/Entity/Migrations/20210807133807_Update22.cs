using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 22
    /// </summary>
    public partial class Update22 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>("LineupExperienceLevelId", "RaidRegistrations", "bigint", nullable: true);

            migrationBuilder.AddColumn<long>("ParticipationPoints",
                                             "RaidExperienceLevels",
                                             "bigint",
                                             nullable: false,
                                             defaultValue: 0L);

            migrationBuilder.AddColumn<int>("Rank",
                                            "RaidExperienceLevels",
                                            "int",
                                            nullable: false,
                                            defaultValue: 0);

            migrationBuilder.AlterColumn<double>("GuildPoints",
                                                 "CalendarAppointmentTemplates",
                                                 "float",
                                                 nullable: true,
                                                 oldClrType: typeof(int),
                                                 oldType: "int",
                                                 oldNullable: true);

            migrationBuilder.CreateTable("Accounts",
                                         table => new
                                                  {
                                                      Name = table.Column<string>("nvarchar(42)", maxLength: 42, nullable: false),
                                                      UserId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      DpsReportUserToken = table.Column<string>("nvarchar(max)", nullable: true),
                                                      ApiKey = table.Column<string>("nvarchar(max)", nullable: true)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_Accounts", x => x.Name);

                                                          table.ForeignKey("FK_Accounts_Users_UserId",
                                                                           x => x.UserId,
                                                                           "Users",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable("RaidCurrentUserPoints",
                                         table => new
                                                  {
                                                      UserId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      Points = table.Column<double>("float", nullable: false),
                                                      UserId1 = table.Column<decimal>("decimal(20,0)", nullable: true)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_RaidCurrentUserPoints", x => x.UserId);

                                                          table.ForeignKey("FK_RaidCurrentUserPoints_Users_UserId1",
                                                                           x => x.UserId1,
                                                                           "Users",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable("RaidRoleLineupHeaders",
                                         table => new
                                                  {
                                                      Id = table.Column<long>("bigint", nullable: false)
                                                                .Annotation("SqlServer:Identity", "1, 1"),
                                                      Description = table.Column<string>("nvarchar(max)", nullable: true)
                                                  },
                                         constraints: table => table.PrimaryKey("PK_RaidRoleLineupHeaders", x => x.Id));

            migrationBuilder.CreateTable("RaidRoleLineupAssignments",
                                         table => new
                                                  {
                                                      TemplateId = table.Column<long>("bigint", nullable: false),
                                                      LineupHeaderId = table.Column<long>("bigint", nullable: false)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_RaidRoleLineupAssignments",
                                                                           x => new
                                                                                {
                                                                                    x.TemplateId,
                                                                                    x.LineupHeaderId
                                                                                });

                                                          table.ForeignKey("FK_RaidRoleLineupAssignments_RaidDayTemplates_TemplateId",
                                                                           x => x.TemplateId,
                                                                           "RaidDayTemplates",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey("FK_RaidRoleLineupAssignments_RaidRoleLineupHeaders_LineupHeaderId",
                                                                           x => x.LineupHeaderId,
                                                                           "RaidRoleLineupHeaders",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable("RaidRoleLineupEntries",
                                         table => new
                                                  {
                                                      LineupHeaderId = table.Column<long>("bigint", nullable: false),
                                                      Position = table.Column<long>("bigint", nullable: false),
                                                      RoleId = table.Column<long>("bigint", nullable: false)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_RaidRoleLineupEntries",
                                                                           x => new
                                                                                {
                                                                                    x.LineupHeaderId,
                                                                                    x.Position,
                                                                                    x.RoleId
                                                                                });

                                                          table.ForeignKey("FK_RaidRoleLineupEntries_RaidRoleLineupHeaders_LineupHeaderId",
                                                                           x => x.LineupHeaderId,
                                                                           "RaidRoleLineupHeaders",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey("FK_RaidRoleLineupEntries_RaidRoles_RoleId",
                                                                           x => x.RoleId,
                                                                           "RaidRoles",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
            migrationBuilder.CreateIndex("IX_RaidRegistrations_LineupExperienceLevelId", "RaidRegistrations", "LineupExperienceLevelId");
            migrationBuilder.CreateIndex("IX_Accounts_UserId", "Accounts", "UserId");
            migrationBuilder.CreateIndex("IX_RaidCurrentUserPoints_UserId1", "RaidCurrentUserPoints", "UserId1");
            migrationBuilder.CreateIndex("IX_RaidRoleLineupAssignments_LineupHeaderId", "RaidRoleLineupAssignments", "LineupHeaderId");
            migrationBuilder.CreateIndex("IX_RaidRoleLineupEntries_RoleId", "RaidRoleLineupEntries", "RoleId");

            migrationBuilder.AddForeignKey("FK_RaidRegistrations_RaidExperienceLevels_LineupExperienceLevelId",
                                           "RaidRegistrations",
                                           "LineupExperienceLevelId",
                                           "RaidExperienceLevels",
                                           principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_RaidRegistrations_RaidExperienceLevels_LineupExperienceLevelId", "RaidRegistrations");
            migrationBuilder.DropTable("Accounts");
            migrationBuilder.DropTable("RaidCurrentUserPoints");
            migrationBuilder.DropTable("RaidRoleLineupAssignments");
            migrationBuilder.DropTable("RaidRoleLineupEntries");
            migrationBuilder.DropTable("RaidRoleLineupHeaders");
            migrationBuilder.DropIndex("IX_RaidRegistrations_LineupExperienceLevelId", "RaidRegistrations");
            migrationBuilder.DropColumn("LineupExperienceLevelId", "RaidRegistrations");
            migrationBuilder.DropColumn("ParticipationPoints", "RaidExperienceLevels");
            migrationBuilder.DropColumn("Rank", "RaidExperienceLevels");

            migrationBuilder.AlterColumn<int>("GuildPoints",
                                              "CalendarAppointmentTemplates",
                                              "int",
                                              nullable: true,
                                              oldClrType: typeof(double),
                                              oldType: "float",
                                              oldNullable: true);
        }
    }
}