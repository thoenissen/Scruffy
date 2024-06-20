using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 34
    /// </summary>
    public partial class Update34 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(name: "DiscordIgnoreChannels",
                                         columns: table => new
                                                           {
                                                               ServerId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               ChannelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_DiscordIgnoreChannels",
                                                                           x => new
                                                                                {
                                                                                    x.ServerId,
                                                                                    x.ChannelId
                                                                                });
                                                      });

            migrationBuilder.CreateTable(name: "DiscordMessages",
                                         columns: table => new
                                                           {
                                                               ServerId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               ChannelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               MessageId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               UserId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                                                               IsBatchCommitted = table.Column<bool>(type: "bit", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_DiscordMessages",
                                                                           x => new
                                                                                {
                                                                                    x.ServerId,
                                                                                    x.ChannelId,
                                                                                    x.MessageId
                                                                                });
                                                      });

            migrationBuilder.CreateTable(name: "DiscordVoiceTimeSpans",
                                         columns: table => new
                                                           {
                                                               ServerId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               ChannelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               UserId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               StartTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                                                               EndTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                                                               IsCompleted = table.Column<bool>(type: "bit", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_DiscordVoiceTimeSpans",
                                                                           x => new
                                                                                {
                                                                                    x.ServerId,
                                                                                    x.ChannelId,
                                                                                    x.UserId,
                                                                                    x.StartTimeStamp
                                                                                });
                                                      });
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DiscordIgnoreChannels");
            migrationBuilder.DropTable(name: "DiscordMessages");
            migrationBuilder.DropTable(name: "DiscordVoiceTimeSpans");
        }
    }
}