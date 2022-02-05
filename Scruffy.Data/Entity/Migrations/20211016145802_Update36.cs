using Microsoft.EntityFrameworkCore.Migrations;

using Scruffy.Data.Enumerations.CoreData;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 36
    /// </summary>
    public partial class Update36 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_CalendarAppointmentTemplates_ServerConfigurations_ServerId", table: "CalendarAppointmentTemplates");
            migrationBuilder.DropForeignKey("FK_RaidCurrentUserPoints_Users_UserId", "RaidCurrentUserPoints");
            migrationBuilder.DropForeignKey("FK_RaidRegistrations_Users_UserId", "RaidRegistrations");
            migrationBuilder.DropForeignKey("FK_RaidUserRoles_Users_UserId", "RaidUserRoles");
            migrationBuilder.DropForeignKey("FK_GuildSpecialRankProtocolEntries_Users_UserId", "GuildSpecialRankProtocolEntries");
            migrationBuilder.DropForeignKey("FK_GuildSpecialRankPoints_Users_UserId", "GuildSpecialRankPoints");
            migrationBuilder.DropForeignKey("FK_Accounts_Users_UserId", "Accounts");
            migrationBuilder.DropForeignKey("FK_CalendarAppointmentParticipants_Users_UserId", "CalendarAppointmentParticipants");
            migrationBuilder.DropForeignKey("FK_OneTimeReminders_Users_UserId", "OneTimeReminders");
            migrationBuilder.DropForeignKey("FK_Users_RaidExperienceLevels_RaidExperienceLevelId", "Users");

            migrationBuilder.DropPrimaryKey("PK_Users", "Users");
            migrationBuilder.DropPrimaryKey("PK_RaidUserRoles", "RaidUserRoles");
            migrationBuilder.DropPrimaryKey("PK_RaidCurrentUserPoints", "RaidCurrentUserPoints");
            migrationBuilder.DropPrimaryKey("PK_GuildSpecialRankPoints", "GuildSpecialRankPoints");
            migrationBuilder.DropPrimaryKey("PK_FractalRegistrations", "FractalRegistrations");
            migrationBuilder.DropPrimaryKey("PK_CalendarAppointmentParticipants", "CalendarAppointmentParticipants");

            migrationBuilder.DropIndex(name: "IX_OneTimeReminders_UserId", table: "OneTimeReminders");

            migrationBuilder.RenameColumn(name: "MessageId", table: "WeeklyReminders", newName: "DiscordMessageId");
            migrationBuilder.RenameColumn(name: "ChannelId", table: "WeeklyReminders", newName: "DiscordChannelId");
            migrationBuilder.RenameColumn(name: "AdministratorRoleId", table: "ServerConfigurations", newName: "DiscordAdministratorRoleId");
            migrationBuilder.RenameColumn(name: "ServerId", table: "ServerConfigurations", newName: "DiscordServerId");
            migrationBuilder.RenameColumn(name: "MessageId", table: "RaidDayConfigurations", newName: "DiscordMessageId");
            migrationBuilder.RenameColumn(name: "ChannelId", table: "RaidDayConfigurations", newName: "DiscordChannelId");
            migrationBuilder.RenameColumn(name: "UserId", table: "OneTimeReminders", newName: "DiscordAccountId");
            migrationBuilder.RenameColumn(name: "ChannelId", table: "OneTimeReminders", newName: "DiscordChannelId");
            migrationBuilder.RenameColumn(name: "MessageId", table: "GuildChannelConfigurations", newName: "DiscordMessageId");
            migrationBuilder.RenameColumn(name: "ChannelId", table: "GuildChannelConfigurations", newName: "DiscordChannelId");
            migrationBuilder.RenameColumn(name: "MessageId", table: "FractalLfgConfigurations", newName: "DiscordMessageId");
            migrationBuilder.RenameColumn(name: "ChannelId", table: "FractalLfgConfigurations", newName: "DiscordChannelId");
            migrationBuilder.RenameColumn(name: "MessageId", table: "FractalAppointments", newName: "DiscordMessageId");
            migrationBuilder.RenameColumn(name: "UserId", table: "DiscordVoiceTimeSpans", newName: "DiscordAccountId");
            migrationBuilder.RenameColumn(name: "ChannelId", table: "DiscordVoiceTimeSpans", newName: "DiscordChannelId");
            migrationBuilder.RenameColumn(name: "ServerId", table: "DiscordVoiceTimeSpans", newName: "DiscordServerId");
            migrationBuilder.RenameColumn(name: "UserId", table: "DiscordMessages", newName: "DiscordAccountId");
            migrationBuilder.RenameColumn(name: "MessageId", table: "DiscordMessages", newName: "DiscordMessageId");
            migrationBuilder.RenameColumn(name: "ChannelId", table: "DiscordMessages", newName: "DiscordChannelId");
            migrationBuilder.RenameColumn(name: "ServerId", table: "DiscordMessages", newName: "DiscordServerId");
            migrationBuilder.RenameColumn(name: "ChannelId", table: "DiscordIgnoreChannels", newName: "DiscordChannelId");
            migrationBuilder.RenameColumn(name: "ServerId", table: "DiscordIgnoreChannels", newName: "DiscordServerId");
            migrationBuilder.RenameColumn(name: "ServerId", table: "CalendarAppointmentTemplates", newName: "DiscordServerId");
            migrationBuilder.RenameColumn(name: "ServerId", table: "CalendarAppointmentSchedules", newName: "DiscordServerId");
            migrationBuilder.RenameColumn(name: "ReminderMessageId", table: "CalendarAppointments", newName: "DiscordMessageId");
            migrationBuilder.RenameColumn(name: "ReminderChannelId", table: "CalendarAppointments", newName: "DiscordChannelId");

            migrationBuilder.RenameIndex(name: "IX_CalendarAppointmentTemplates_ServerId", table: "CalendarAppointmentTemplates", newName: "IX_CalendarAppointmentTemplates_DiscordServerId");

            migrationBuilder.RenameTable(name: "Users", newName: "Users_Temp");

            migrationBuilder.CreateTable("Users",
                                         table => new
                                         {
                                             Id = table.Column<long>("bigint", nullable: false)
                                                                .Annotation("SqlServer:Identity", "1, 1"),
                                             CreationTimeStamp = table.Column<DateTime>("datetime2", nullable: false),
                                             Type = table.Column<UserType>("int", nullable: false),
                                             RaidExperienceLevelId = table.Column<long?>("bigint", nullable: true)
                                         },
                                         constraints: table => table.PrimaryKey("PK_Users", x => x.Id));
            migrationBuilder.CreateTable(name: "DiscordAccounts",
                                         columns: table => new
                                         {
                                             Id = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                             UserId = table.Column<long>(type: "bigint", nullable: false)
                                         },
                                         constraints: table =>
                                         {
                                             table.PrimaryKey("PK_DiscordAccounts", x => x.Id);

                                             table.ForeignKey(name: "FK_DiscordAccounts_Users_UserId",
                                                              column: x => x.UserId,
                                                              principalTable: "Users",
                                                              principalColumn: "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });
            migrationBuilder.CreateTable(name: "GuildWarsAccounts",
                                         columns: table => new
                                         {
                                             Name = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                                             UserId = table.Column<long>(type: "bigint", nullable: false),
                                             DpsReportUserToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                             ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                             LastAge = table.Column<long>(type: "bigint", nullable: false),
                                             WorldId = table.Column<long>(type: "bigint", nullable: true)
                                         },
                                         constraints: table =>
                                         {
                                             table.PrimaryKey("PK_GuildWarsAccounts", x => x.Name);

                                             table.ForeignKey(name: "FK_GuildWarsAccounts_Users_UserId",
                                                              column: x => x.UserId,
                                                              principalTable: "Users",
                                                              principalColumn: "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateTable(name: "GuildWarsAccountDailyLoginChecks",
                                         columns: table => new
                                                           {
                                                               Name = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                                                               Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildWarsAccountDailyLoginChecks",
                                                                           x => new
                                                                                {
                                                                                    x.Name,
                                                                                    x.Date
                                                                                });

                                                          table.ForeignKey(name: "FK_GuildWarsAccountDailyLoginChecks_GuildWarsAccounts_Name",
                                                                           column: x => x.Name,
                                                                           principalTable: "GuildWarsAccounts",
                                                                           principalColumn: "Name",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.AddColumn<int>(name: "NewId", table: "Users_Temp", type: "int", nullable: false, defaultValue: 0);

            migrationBuilder.Sql(@"UPDATE [U] 
                                   SET [U].[NewId] = [RowNumber]
                                   FROM ( SELECT [NewId], ROW_NUMBER() OVER ( ORDER BY [CreationTimeStamp] ) AS [RowNumber] FROM [Users_Temp] ) AS [U]");

            migrationBuilder.Sql(@"UPDATE [U]
                                      SET [U].[UserId] = [T].[NewId]
                                     FROM [RaidUserRoles] AS [U]
                               INNER JOIN [Users_Temp] AS [T]
                                       ON [T].[Id] = [U].[UserId]");
            migrationBuilder.Sql(@"UPDATE [U]
                                      SET [U].[UserId] = [T].[NewId]
                                     FROM [RaidRegistrations] AS [U]
                               INNER JOIN [Users_Temp] AS [T]
                                       ON [T].[Id] = [U].[UserId]");
            migrationBuilder.Sql(@"UPDATE [U]
                                      SET [U].[UserId] = [T].[NewId]
                                     FROM [RaidCurrentUserPoints] AS [U]
                               INNER JOIN [Users_Temp] AS [T]
                                       ON [T].[Id] = [U].[UserId]");
            migrationBuilder.Sql(@"UPDATE [U]
                                      SET [U].[UserId] = [T].[NewId]
                                     FROM [GuildSpecialRankProtocolEntries] AS [U]
                               INNER JOIN [Users_Temp] AS [T]
                                       ON [T].[Id] = [U].[UserId]");
            migrationBuilder.Sql(@"UPDATE [U]
                                      SET [U].[UserId] = [T].[NewId]
                                     FROM [GuildSpecialRankPoints] AS [U]
                               INNER JOIN [Users_Temp] AS [T]
                                       ON [T].[Id] = [U].[UserId]");
            migrationBuilder.Sql(@"UPDATE [U]
                                      SET [U].[UserId] = [T].[NewId]
                                     FROM [FractalRegistrations] AS [U]
                               INNER JOIN [Users_Temp] AS [T]
                                       ON [T].[Id] = [U].[UserId]");
            migrationBuilder.Sql(@"UPDATE [U]
                                      SET [U].[UserId] = [T].[NewId]
                                     FROM [CalendarAppointmentParticipants] AS [U]
                               INNER JOIN [Users_Temp] AS [T]
                                       ON [T].[Id] = [U].[UserId]");

            migrationBuilder.AlterColumn<long>(name: "UserId", table: "RaidUserRoles", type: "bigint", nullable: false, oldClrType: typeof(decimal), oldType: "decimal(20,0)");
            migrationBuilder.AlterColumn<long>(name: "UserId", table: "RaidRegistrations", type: "bigint", nullable: false, oldClrType: typeof(decimal), oldType: "decimal(20,0)");
            migrationBuilder.AlterColumn<long>(name: "UserId", table: "RaidCurrentUserPoints", type: "bigint", nullable: false, oldClrType: typeof(decimal), oldType: "decimal(20,0)");
            migrationBuilder.AlterColumn<long>(name: "UserId", table: "GuildSpecialRankProtocolEntries", type: "bigint", nullable: true, oldClrType: typeof(decimal), oldType: "decimal(20,0)", oldNullable: true);
            migrationBuilder.AlterColumn<long>(name: "UserId", table: "GuildSpecialRankPoints", type: "bigint", nullable: false, oldClrType: typeof(decimal), oldType: "decimal(20,0)");
            migrationBuilder.AlterColumn<long>(name: "UserId", table: "FractalRegistrations", type: "bigint", nullable: false, oldClrType: typeof(decimal), oldType: "decimal(20,0)");
            migrationBuilder.AlterColumn<long>(name: "UserId", table: "CalendarAppointmentParticipants", type: "bigint", nullable: false, oldClrType: typeof(decimal), oldType: "decimal(20,0)");

            migrationBuilder.Sql("SET IDENTITY_INSERT [Users] ON");

            migrationBuilder.Sql(@"INSERT INTO [Users] ([Id], [CreationTimeStamp], [Type], [RaidExperienceLevelId])
                                   SELECT[NewId], [CreationTimeStamp], 0, [RaidExperienceLevelId] FROM[Users_Temp]");

            migrationBuilder.Sql(@"DECLARE @id as INT = (SELECT TOP 1 [Id] FROM [Users] ORDER BY [Id] DESC);
                                   DBCC CHECKIDENT ('Users', RESEED, @id)");

            migrationBuilder.Sql("SET IDENTITY_INSERT [Users] OFF");

            migrationBuilder.Sql(@"INSERT INTO [DiscordAccounts]
                                   SELECT [Id], [NewId] FROM [Users_Temp]");

            migrationBuilder.Sql(@"INSERT INTO [GuildWarsAccounts]
                                        SELECT [A].[Name], [U].[NewId], [A].[DpsReportUserToken], [A].[ApiKey], [A].[LastAge], [A].[WordId] FROM [Accounts] AS [A]
								    INNER JOIN [Users_Temp] AS [U]
								            ON [U].[Id] = [A].[UserId]");

            migrationBuilder.Sql(@"INSERT INTO [GuildWarsAccountDailyLoginChecks]
                                   SELECT [Name], [Date] FROM [AccountDailyLoginChecks]");

            migrationBuilder.CreateIndex(name: "IX_OneTimeReminders_DiscordAccountId", table: "OneTimeReminders", column: "DiscordAccountId");
            migrationBuilder.CreateIndex(name: "IX_FractalRegistrations_UserId", table: "FractalRegistrations", column: "UserId");
            migrationBuilder.CreateIndex(name: "IX_DiscordVoiceTimeSpans_DiscordAccountId", table: "DiscordVoiceTimeSpans", column: "DiscordAccountId");
            migrationBuilder.CreateIndex(name: "IX_DiscordAccounts_UserId", table: "DiscordAccounts", column: "UserId");
            migrationBuilder.CreateIndex(name: "IX_GuildWarsAccounts_UserId", table: "GuildWarsAccounts", column: "UserId");

            migrationBuilder.AddPrimaryKey(name: "PK_RaidUserRoles", "RaidUserRoles", new[] { "UserId", "MainRoleId", "SubRoleId" });
            migrationBuilder.AddPrimaryKey(name: "PK_RaidCurrentUserPoints", "RaidCurrentUserPoints", "UserId");
            migrationBuilder.AddPrimaryKey(name: "PK_GuildSpecialRankPoints", "GuildSpecialRankPoints", new[] { "ConfigurationId", "UserId" });
            migrationBuilder.AddPrimaryKey(name: "PK_FractalRegistrations", "FractalRegistrations", new[] { "ConfigurationId", "AppointmentTimeStamp", "UserId" });
            migrationBuilder.AddPrimaryKey(name: "PK_CalendarAppointmentParticipants", "CalendarAppointmentParticipants", new[] { "AppointmentId", "UserId" });

            migrationBuilder.AddForeignKey(name: "FK_Users_RaidExperienceLevels_RaidExperienceLevelId", "Users", "RaidExperienceLevelId", "RaidExperienceLevels", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_RaidUserRoles_Users_UserId", table: "RaidUserRoles", column: "UserId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_RaidRegistrations_Users_UserId", table: "RaidRegistrations", column: "UserId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_RaidCurrentUserPoints_Users_UserId", table: "RaidCurrentUserPoints", column: "UserId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_GuildSpecialRankProtocolEntries_Users_UserId", table: "GuildSpecialRankProtocolEntries", column: "UserId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_GuildSpecialRankPoints_Users_UserId", table: "GuildSpecialRankPoints", column: "UserId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_CalendarAppointmentParticipants_Users_UserId", table: "CalendarAppointmentParticipants", column: "UserId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_CalendarAppointmentTemplates_ServerConfigurations_DiscordServerId", table: "CalendarAppointmentTemplates", column: "DiscordServerId", principalTable: "ServerConfigurations", principalColumn: "DiscordServerId", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_DiscordVoiceTimeSpans_DiscordAccounts_DiscordAccountId", table: "DiscordVoiceTimeSpans", column: "DiscordAccountId", principalTable: "DiscordAccounts", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_FractalRegistrations_Users_UserId", table: "FractalRegistrations", column: "UserId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_OneTimeReminders_DiscordAccounts_DiscordAccountId", table: "OneTimeReminders", column: "DiscordAccountId", principalTable: "DiscordAccounts", principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropTable("AccountDailyLoginChecks");
            migrationBuilder.DropTable("Accounts");
            migrationBuilder.DropTable("Users_Temp");
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException();
        }
    }
}