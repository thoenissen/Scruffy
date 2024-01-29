using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 57
    /// </summary>
    public partial class Update57 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(name: "GuildDonations",
                                         columns: table => new
                                                           {
                                                               GuildId = table.Column<long>(type: "bigint", nullable: false),
                                                               LogEntryId = table.Column<int>(type: "int", nullable: false),
                                                               Value = table.Column<long>(type: "bigint", nullable: false),
                                                               IsThresholdRelevant = table.Column<bool>(type: "bit", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildDonations",
                                                                           x => new
                                                                                {
                                                                                    x.GuildId,
                                                                                    x.LogEntryId
                                                                                });
                                                      });

            migrationBuilder.CreateTable(name: "GuildWarsCustomRecipeEntries",
                                         columns: table => new
                                                           {
                                                               ItemId = table.Column<int>(type: "int", nullable: false),
                                                               IngredientItemId = table.Column<int>(type: "int", nullable: false),
                                                               IngredientCount = table.Column<int>(type: "int", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildWarsCustomRecipeEntries",
                                                                           x => new
                                                                                {
                                                                                    x.ItemId,
                                                                                    x.IngredientItemId
                                                                                });
                                                      });

            migrationBuilder.Sql(@"CREATE FUNCTION [dbo].[ScruffyGetWeeklyEventPoints] (
                                       @from DATETIME2(7),
                                       @to   DATETIME2(7)
                                   )
                                   RETURNS TABLE
                                   
                                   AS
                                       RETURN SELECT [Week],
                                                     POWER ( (CAST ( 10.0 AS FLOAT ) ), - ( ( [Week] - 1 ) * 2  - 15.0 ) / 14.6) AS [Weight],
                                                     SUM ( [Raw].[GuildPoints] ) AS [Points]
                                                FROM ( SELECT ( DATEDIFF ( DAY, [Appointment].[TimeStamp], @to ) / 7) + 1  AS [Week],
                                                              [Template].[GuildPoints]
                                                         FROM [CalendarAppointments] AS [Appointment]
                                                   INNER JOIN [CalendarAppointmentTemplates] AS [Template]
                                                           ON [Template].[Id]  = [Appointment].[CalendarAppointmentTemplateId]
                                                        WHERE [Appointment].[TimeStamp] > @from
                                                          AND [Appointment].[TimeStamp] < @to
                                                          AND [Template].[IsRaisingGuildPointCap] = 1 ) AS [Raw]
                                            GROUP BY [Week]");

            migrationBuilder.Sql(@"CREATE FUNCTION [dbo].[ScruffyGetWeeklyDonationReferences] (
                                       @guildId INT,
                                       @from    DATETIME2(7),
                                       @to      DATETIME2(7)
                                   )
                                   RETURNS TABLE
                                   
                                   AS
                                       RETURN SELECT [Week],
                                                     POWER ( (CAST ( 10.0 AS FLOAT ) ), - ( ( [Week] - 1 ) * 2  - 15.0 ) / 14.6) AS [Weight],
                                                     SUM ( [Raw].[Value] ) 
                                                       / ( SELECT COUNT(DISTINCT [Account].[UserId]) 
                                                             FROM [GuildDonations] AS [UserDonation]
                                                       INNER JOIN [GuildLogEntries] AS [UserLogEntry]
                                                               ON [UserLogEntry].[Id]  = [UserDonation].[LogEntryId]
                                                       INNER JOIN [GuildWarsAccounts] AS [Account]
                                                               ON [Account].[Name] = [UserLogEntry].[User]
                                                            WHERE [UserLogEntry].[Time] > @from
                                                              AND [UserLogEntry].[Time] < @to )
                                                       / 5 AS [Value]
                                                FROM ( SELECT ( DATEDIFF ( DAY, [LogEntry].[Time], @to ) / 7) + 1  AS [Week],
                                                              [Donation].[Value]
                                                         FROM [GuildDonations] AS [Donation]
                                                   INNER JOIN [GuildLogEntries] AS [LogEntry]
                                                           ON [LogEntry].[Id]  = [Donation].[LogEntryId]
                                                        WHERE [LogEntry].[Time] > @from
                                                          AND [LogEntry].[Time] < @to 
                                                          AND [Donation].[Value] > 0 ) AS [Raw]
                                            GROUP BY [Week]");

            migrationBuilder.Sql(@"CREATE  FUNCTION [dbo].[ScruffyGetWeeklyCommits] (
                                       @guildId INT,
                                       @from    DATETIME2(7),
                                       @to      DATETIME2(7)
                                   )
                                   RETURNS TABLE
                                   
                                   AS
                                       RETURN SELECT [Week],
                                                     POWER ( (CAST ( 10.0 AS FLOAT ) ), - ( ( [Week] - 1 ) * 2  - 15.0 ) / 14.6) AS [Weight],
                                                     COUNT(*) 
                                                     / CAST ( ( SELECT COUNT ( DISTINCT [Author] ) 
                                                                  FROM [GitHubCommits] AS [UserCommit]
                                                                 WHERE [UserCommit].[TimeStamp] > @from
                                                                   AND [UserCommit].[TimeStamp] < @to ) AS FLOAT )
                                                     / 5 AS [Count]
                                                FROM ( SELECT ( DATEDIFF ( DAY, [Commit].[TimeStamp], @to ) / 7) + 1  AS [Week]
                                                         FROM [GitHubCommits] AS [Commit]
                                                        WHERE [Commit].[TimeStamp] > @from
                                                          AND [Commit].[TimeStamp] < @to ) AS [Raw]
                                            GROUP BY [Week]");
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GuildDonations");
            migrationBuilder.DropTable(name: "GuildWarsCustomRecipeEntries");
        }
    }
}