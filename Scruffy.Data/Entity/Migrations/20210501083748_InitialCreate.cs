using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Initial creation
    /// </summary>
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("RaidDayConfigurations",
                                         table => new
                                                  {
                                                      Id = table.Column<long>("bigint", nullable: false)
                                                                .Annotation("SqlServer:Identity", "1, 1"),
                                                      Day = table.Column<int>("int", nullable: false),
                                                      RegistrationDeadline = table.Column<TimeSpan>("time", nullable: false),
                                                      StartTime = table.Column<TimeSpan>("time", nullable: false),
                                                      ResetTime = table.Column<TimeSpan>("time", nullable: false),
                                                      ReminderTime = table.Column<TimeSpan>("time", nullable: true),
                                                      ReminderChannelId = table.Column<decimal>("decimal(20,0)", nullable: true),
                                                      ChannelId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      MessageId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      AdministrationRoleId = table.Column<decimal>("decimal(20,0)", nullable: true)
                                                  },
                                         constraints: table => table.PrimaryKey("PK_RaidDayConfigurations", x => x.Id));

            migrationBuilder.CreateTable("RaidExperienceLevels",
                                         table => new
                                                  {
                                                      Id = table.Column<long>("bigint", nullable: false)
                                                                .Annotation("SqlServer:Identity", "1, 1"),
                                                      SuperiorExperienceLevelId = table.Column<long>("bigint", nullable: false),
                                                      DiscordRoleId = table.Column<decimal>("decimal(20,0)", nullable: true)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_RaidExperienceLevels", x => x.Id);

                                                          table.ForeignKey("FK_RaidExperienceLevels_RaidExperienceLevels_SuperiorExperienceLevelId",
                                                                           x => x.SuperiorExperienceLevelId,
                                                                           "RaidExperienceLevels",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable("RaidRoles",
                                         table => new
                                                  {
                                                      Id = table.Column<long>("bigint", nullable: false)
                                                                .Annotation("SqlServer:Identity", "1, 1"),
                                                      MainRoleId = table.Column<long>("bigint", nullable: false),
                                                      DiscordEmojiId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      Description = table.Column<string>("nvarchar(max)", nullable: true)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_RaidRoles", x => x.Id);

                                                          table.ForeignKey("FK_RaidRoles_RaidRoles_MainRoleId",
                                                                           x => x.MainRoleId,
                                                                           "RaidRoles",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable("Users",
                                         table => new
                                                  {
                                                      Id = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      CreationTimeStamp = table.Column<DateTime>("datetime2", nullable: false)
                                                  },
                                         constraints: table => table.PrimaryKey("PK_Users", x => x.Id));

            migrationBuilder.CreateTable("RaidAppointments",
                                         table => new
                                                  {
                                                      Id = table.Column<long>("bigint", nullable: false)
                                                                .Annotation("SqlServer:Identity", "1, 1"),
                                                      ConfigurationId = table.Column<long>("bigint", nullable: false),
                                                      TimeStamp = table.Column<DateTime>("datetime2", nullable: false)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_RaidAppointments", x => x.Id);

                                                          table.ForeignKey("FK_RaidAppointments_RaidDayConfigurations_ConfigurationId",
                                                                           x => x.ConfigurationId,
                                                                           "RaidDayConfigurations",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable("RaidExperienceAssignments",
                                         table => new
                                                  {
                                                      ConfigurationId = table.Column<long>("bigint", nullable: false),
                                                      ExperienceLevelId = table.Column<long>("bigint", nullable: false),
                                                      Count = table.Column<long>("bigint", nullable: false)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_RaidExperienceAssignments",
                                                                           x => new
                                                                                {
                                                                                    x.ConfigurationId,
                                                                                    x.ExperienceLevelId
                                                                                });

                                                          table.ForeignKey("FK_RaidExperienceAssignments_RaidDayConfigurations_ConfigurationId",
                                                                           x => x.ConfigurationId,
                                                                           "RaidDayConfigurations",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey("FK_RaidExperienceAssignments_RaidExperienceLevels_ExperienceLevelId",
                                                                           x => x.ExperienceLevelId,
                                                                           "RaidExperienceLevels",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable("RaidRequiredRoles",
                                         table => new
                                                  {
                                                      ConfigurationId = table.Column<long>("bigint", nullable: false),
                                                      Index = table.Column<long>("bigint", nullable: false),
                                                      MainRoleId = table.Column<long>("bigint", nullable: false),
                                                      SubRoleId = table.Column<long>("bigint", nullable: true)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_RaidRequiredRoles",
                                                                           x => new
                                                                                {
                                                                                    x.ConfigurationId,
                                                                                    x.Index
                                                                                });

                                                          table.ForeignKey("FK_RaidRequiredRoles_RaidDayConfigurations_ConfigurationId",
                                                                           x => x.ConfigurationId,
                                                                           "RaidDayConfigurations",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey("FK_RaidRequiredRoles_RaidRoles_MainRoleId",
                                                                           x => x.MainRoleId,
                                                                           "RaidRoles",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey("FK_RaidRequiredRoles_RaidRoles_SubRoleId",
                                                                           x => x.SubRoleId,
                                                                           "RaidRoles",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable("RaidRoleAliasNames",
                                         table => new
                                                  {
                                                      AliasName = table.Column<string>("nvarchar(20)", maxLength: 20, nullable: false),
                                                      MainRoleId = table.Column<long>("bigint", nullable: false),
                                                      SubRoleId = table.Column<long>("bigint", nullable: true)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_RaidRoleAliasNames", x => x.AliasName);

                                                          table.ForeignKey("FK_RaidRoleAliasNames_RaidRoles_MainRoleId",
                                                                           x => x.MainRoleId,
                                                                           "RaidRoles",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey("FK_RaidRoleAliasNames_RaidRoles_SubRoleId",
                                                                           x => x.SubRoleId,
                                                                           "RaidRoles",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable("OneTimeReminders",
                                         table => new
                                                  {
                                                      Id = table.Column<long>("bigint", nullable: false)
                                                                .Annotation("SqlServer:Identity", "1, 1"),
                                                      UserId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      ChannelId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      TimeStamp = table.Column<DateTime>("datetime2", nullable: false),
                                                      Message = table.Column<string>("nvarchar(max)", nullable: true),
                                                      IsExecuted = table.Column<bool>("bit", nullable: false)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_OneTimeReminders", x => x.Id);

                                                          table.ForeignKey("FK_OneTimeReminders_Users_UserId",
                                                                           x => x.UserId,
                                                                           "Users",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable("RaidUserRoles",
                                         table => new
                                                  {
                                                      UserId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      MainRoleId = table.Column<long>("bigint", nullable: false),
                                                      SubRoleId = table.Column<long>("bigint", nullable: false)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_RaidUserRoles",
                                                                           x => new
                                                                                {
                                                                                    x.UserId,
                                                                                    x.MainRoleId,
                                                                                    x.SubRoleId
                                                                                });

                                                          table.ForeignKey("FK_RaidUserRoles_RaidRoles_MainRoleId",
                                                                           x => x.MainRoleId,
                                                                           "RaidRoles",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey("FK_RaidUserRoles_RaidRoles_SubRoleId",
                                                                           x => x.SubRoleId,
                                                                           "RaidRoles",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey("FK_RaidUserRoles_Users_UserId",
                                                                           x => x.UserId,
                                                                           "Users",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable("RaidRegistrations",
                                         table => new
                                                  {
                                                      Id = table.Column<long>("bigint", nullable: false)
                                                                .Annotation("SqlServer:Identity", "1, 1"),
                                                      AppointmentId = table.Column<long>("bigint", nullable: false),
                                                      UserId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      RegistrationTimeStamp = table.Column<DateTime>("datetime2", nullable: false),
                                                      Points = table.Column<long>("bigint", nullable: true)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_RaidRegistrations", x => x.Id);

                                                          table.ForeignKey("FK_RaidRegistrations_RaidAppointments_AppointmentId",
                                                                           x => x.AppointmentId,
                                                                           "RaidAppointments",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey("FK_RaidRegistrations_Users_UserId",
                                                                           x => x.UserId,
                                                                           "Users",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable("RaidRegistrationRoleAssignments",
                                         table => new
                                                  {
                                                      Id = table.Column<long>("bigint", nullable: false)
                                                                .Annotation("SqlServer:Identity", "1, 1"),
                                                      RegistrationId = table.Column<long>("bigint", nullable: false),
                                                      MainRoleId = table.Column<long>("bigint", nullable: true),
                                                      SubRoleId = table.Column<long>("bigint", nullable: true)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_RaidRegistrationRoleAssignments", x => x.Id);

                                                          table.ForeignKey("FK_RaidRegistrationRoleAssignments_RaidRegistrations_RegistrationId",
                                                                           x => x.RegistrationId,
                                                                           "RaidRegistrations",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey("FK_RaidRegistrationRoleAssignments_RaidRoles_MainRoleId",
                                                                           x => x.MainRoleId,
                                                                           "RaidRoles",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey("FK_RaidRegistrationRoleAssignments_RaidRoles_SubRoleId",
                                                                           x => x.SubRoleId,
                                                                           "RaidRoles",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
            migrationBuilder.CreateIndex("IX_OneTimeReminders_UserId", "OneTimeReminders", "UserId");
            migrationBuilder.CreateIndex("IX_RaidAppointments_ConfigurationId", "RaidAppointments", "ConfigurationId");
            migrationBuilder.CreateIndex("IX_RaidExperienceAssignments_ExperienceLevelId", "RaidExperienceAssignments", "ExperienceLevelId");
            migrationBuilder.CreateIndex("IX_RaidExperienceLevels_SuperiorExperienceLevelId", "RaidExperienceLevels", "SuperiorExperienceLevelId");
            migrationBuilder.CreateIndex("IX_RaidRegistrationRoleAssignments_MainRoleId", "RaidRegistrationRoleAssignments", "MainRoleId");
            migrationBuilder.CreateIndex("IX_RaidRegistrationRoleAssignments_RegistrationId", "RaidRegistrationRoleAssignments", "RegistrationId");
            migrationBuilder.CreateIndex("IX_RaidRegistrationRoleAssignments_SubRoleId", "RaidRegistrationRoleAssignments", "SubRoleId");
            migrationBuilder.CreateIndex("IX_RaidRegistrations_AppointmentId", "RaidRegistrations", "AppointmentId");
            migrationBuilder.CreateIndex("IX_RaidRegistrations_UserId", "RaidRegistrations", "UserId");
            migrationBuilder.CreateIndex("IX_RaidRequiredRoles_MainRoleId", "RaidRequiredRoles", "MainRoleId");
            migrationBuilder.CreateIndex("IX_RaidRequiredRoles_SubRoleId", "RaidRequiredRoles", "SubRoleId");
            migrationBuilder.CreateIndex("IX_RaidRoleAliasNames_MainRoleId", "RaidRoleAliasNames", "MainRoleId");
            migrationBuilder.CreateIndex("IX_RaidRoleAliasNames_SubRoleId", "RaidRoleAliasNames", "SubRoleId");
            migrationBuilder.CreateIndex("IX_RaidRoles_MainRoleId", "RaidRoles", "MainRoleId");
            migrationBuilder.CreateIndex("IX_RaidUserRoles_MainRoleId", "RaidUserRoles", "MainRoleId");
            migrationBuilder.CreateIndex("IX_RaidUserRoles_SubRoleId", "RaidUserRoles", "SubRoleId");
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("OneTimeReminders");
            migrationBuilder.DropTable("RaidExperienceAssignments");
            migrationBuilder.DropTable("RaidRegistrationRoleAssignments");
            migrationBuilder.DropTable("RaidRequiredRoles");
            migrationBuilder.DropTable("RaidRoleAliasNames");
            migrationBuilder.DropTable("RaidUserRoles");
            migrationBuilder.DropTable("RaidExperienceLevels");
            migrationBuilder.DropTable("RaidRegistrations");
            migrationBuilder.DropTable("RaidRoles");
            migrationBuilder.DropTable("RaidAppointments");
            migrationBuilder.DropTable("Users");
            migrationBuilder.DropTable("RaidDayConfigurations");
        }
    }
}