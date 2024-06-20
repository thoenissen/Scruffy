using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 15
    /// </summary>
    public partial class Update15 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>("IsCommitted",
                                             "RaidAppointments",
                                             "bit",
                                             nullable: false,
                                             defaultValue: false);

            migrationBuilder.CreateTable("Guilds",
                                         table => new
                                                  {
                                                      Id = table.Column<long>("bigint", nullable: false)
                                                                .Annotation("SqlServer:Identity", "1, 1"),
                                                      ApiKey = table.Column<string>("nvarchar(max)", nullable: true),
                                                      GuildId = table.Column<string>("nvarchar(max)", nullable: true),
                                                      DiscordServerId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      NotificationChannelId = table.Column<decimal>("decimal(20,0)", nullable: true)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_Guilds", x => x.Id);

                                                          table.ForeignKey("FK_Guilds_ServerConfigurationEntity_DiscordServerId",
                                                                           x => x.DiscordServerId,
                                                                           "ServerConfigurationEntity",
                                                                           "ServerId",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable("GuildLogEntries",
                                         table => new
                                                  {
                                                      GuildId = table.Column<long>("bigint", nullable: false),
                                                      Id = table.Column<int>("int", nullable: false),
                                                      Time = table.Column<DateTime>("datetime2", nullable: false),
                                                      Type = table.Column<string>("nvarchar(max)", nullable: true),
                                                      User = table.Column<string>("nvarchar(max)", nullable: true),
                                                      KickedBy = table.Column<string>("nvarchar(max)", nullable: true),
                                                      InvitedBy = table.Column<string>("nvarchar(max)", nullable: true),
                                                      Operation = table.Column<string>("nvarchar(max)", nullable: true),
                                                      ItemId = table.Column<int>("int", nullable: true),
                                                      Count = table.Column<int>("int", nullable: true),
                                                      Coins = table.Column<int>("int", nullable: true),
                                                      ChangedBy = table.Column<string>("nvarchar(max)", nullable: true),
                                                      OldRank = table.Column<string>("nvarchar(max)", nullable: true),
                                                      NewRank = table.Column<string>("nvarchar(max)", nullable: true),
                                                      UpgradeId = table.Column<int>("int", nullable: true),
                                                      RecipeId = table.Column<int>("int", nullable: true),
                                                      Action = table.Column<string>("nvarchar(max)", nullable: true),
                                                      Activity = table.Column<string>("nvarchar(max)", nullable: true),
                                                      TotalParticipants = table.Column<int>("int", nullable: true),
                                                      Participants = table.Column<string>("nvarchar(max)", nullable: true),
                                                      MessageOfTheDay = table.Column<string>("nvarchar(max)", nullable: true)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildLogEntries",
                                                                           x => new
                                                                                {
                                                                                    x.GuildId,
                                                                                    x.Id
                                                                                });

                                                          table.ForeignKey("FK_GuildLogEntries_Guilds_GuildId",
                                                                           x => x.GuildId,
                                                                           "Guilds",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
            migrationBuilder.CreateIndex("IX_Guilds_DiscordServerId", "Guilds", "DiscordServerId");
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("GuildLogEntries");
            migrationBuilder.DropTable("Guilds");
            migrationBuilder.DropColumn("IsCommitted", "RaidAppointments");
        }
    }
}