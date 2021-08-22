using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 29
    /// </summary>
    public partial class Update29 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>("WordId", "Accounts", "bigint", nullable: true);

            migrationBuilder.CreateTable("GuildChannelConfigurations",
                                         table => new
                                                  {
                                                      GuildId = table.Column<long>("bigint", nullable: false),
                                                      Type = table.Column<int>("int", nullable: false),
                                                      ChannelId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      MessageId = table.Column<decimal>("decimal(20,0)", nullable: true),
                                                      AdditionalData = table.Column<string>("nvarchar(max)", nullable: true)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildChannelConfigurations",
                                                                           x => new
                                                                                {
                                                                                    x.GuildId,
                                                                                    x.Type
                                                                                });

                                                          table.ForeignKey("FK_GuildChannelConfigurations_Guilds_GuildId",
                                                                           x => x.GuildId,
                                                                           "Guilds",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable("GuildWarsWorlds",
                                         table => new
                                                  {
                                                      Id = table.Column<long>("bigint", nullable: false),
                                                      Name = table.Column<string>("nvarchar(max)", nullable: true)
                                                  },
                                         constraints: table => table.PrimaryKey("PK_GuildWarsWorlds", x => x.Id));

            migrationBuilder.Sql(@"INSERT INTO [dbo].[GuildChannelConfigurations]
                                   SELECT [Id], 1000, [ReminderChannelId], null, null FROM [dbo].[Guilds] WHERE [ReminderChannelId] IS NOT NULL");

            migrationBuilder.Sql(@"INSERT INTO [dbo].[GuildChannelConfigurations]
                                   SELECT [Id], 1001, [GuildCalendarChannelId], [GuildCalendarMessageId], '{ ""' + CalendarTitle + '"": ""' + CalendarDescription + '"", ""Description"": ""Test2""}' FROM [dbo].[Guilds] WHERE [GuildCalendarChannelId] IS NOT NULL");

            migrationBuilder.Sql(@"INSERT INTO [dbo].[GuildChannelConfigurations]
                                   SELECT [Id], 1002, [MessageOfTheDayChannelId], [MessageOfTheDayMessageId], null FROM [dbo].[Guilds] WHERE [MessageOfTheDayChannelId] IS NOT NULL");

            migrationBuilder.Sql(@"INSERT INTO [dbo].[GuildChannelConfigurations]
                                   SELECT [Id], 2000, [NotificationChannelId], null, null FROM [dbo].[Guilds] WHERE [NotificationChannelId] IS NOT NULL");

            migrationBuilder.Sql(@"INSERT INTO [dbo].[GuildChannelConfigurations]
                                   SELECT [Id], 3000, [NotificationChannelId], null, null FROM [dbo].[Guilds] WHERE [NotificationChannelId] IS NOT NULL");

            migrationBuilder.DropColumn("CalendarDescription", "Guilds");
            migrationBuilder.DropColumn("CalendarTitle", "Guilds");
            migrationBuilder.DropColumn("GuildCalendarChannelId", "Guilds");
            migrationBuilder.DropColumn("GuildCalendarMessageId", "Guilds");
            migrationBuilder.DropColumn("MessageOfTheDayChannelId", "Guilds");
            migrationBuilder.DropColumn("MessageOfTheDayMessageId", "Guilds");
            migrationBuilder.DropColumn("NotificationChannelId", "Guilds");
            migrationBuilder.DropColumn("ReminderChannelId", "Guilds");
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("GuildChannelConfigurations");
            migrationBuilder.DropTable("GuildWarsWorlds");
            migrationBuilder.DropColumn("WordId", "Accounts");
            migrationBuilder.AddColumn<string>("CalendarDescription", "Guilds", "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<string>("CalendarTitle", "Guilds", "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<decimal>("GuildCalendarChannelId", "Guilds", "decimal(20,0)", nullable: true);
            migrationBuilder.AddColumn<decimal>("GuildCalendarMessageId", "Guilds", "decimal(20,0)", nullable: true);
            migrationBuilder.AddColumn<decimal>("MessageOfTheDayChannelId", "Guilds", "decimal(20,0)", nullable: true);
            migrationBuilder.AddColumn<decimal>("MessageOfTheDayMessageId", "Guilds", "decimal(20,0)", nullable: true);
            migrationBuilder.AddColumn<decimal>("NotificationChannelId", "Guilds", "decimal(20,0)", nullable: true);
            migrationBuilder.AddColumn<decimal>("ReminderChannelId", "Guilds", "decimal(20,0)", nullable: true);
        }
    }
}