using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 38
    /// </summary>
    public partial class Update38 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(name: "GuildWarsAchievements",
                                         columns: table => new
                                                           {
                                                               Id = table.Column<int>(type: "int", nullable: false),
                                                               Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                               Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                               Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                               Requirement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                               LockedText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                               Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                               PointCap = table.Column<int>(type: "int", nullable: true)
                                                           },
                                         constraints: table => table.PrimaryKey("PK_GuildWarsAchievements", x => x.Id));

            migrationBuilder.CreateTable(name: "GuildWarsAchievementBits",
                                         columns: table => new
                                                           {
                                                               AchievementId = table.Column<int>(type: "int", nullable: false),
                                                               Bit = table.Column<int>(type: "int", nullable: false),
                                                               Id = table.Column<int>(type: "int", nullable: true),
                                                               Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                               Text = table.Column<string>(type: "nvarchar(max)", nullable: true)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildWarsAchievementBits",
                                                                           x => new
                                                                                {
                                                                                    x.AchievementId,
                                                                                    x.Bit
                                                                                });

                                                          table.ForeignKey(name: "FK_GuildWarsAchievementBits_GuildWarsAchievements_AchievementId",
                                                                           column: x => x.AchievementId,
                                                                           principalTable: "GuildWarsAchievements",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable(name: "GuildWarsAchievementFlags",
                                         columns: table => new
                                                           {
                                                               AchievementId = table.Column<int>(type: "int", nullable: false),
                                                               Flag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildWarsAchievementFlags",
                                                                           x => new
                                                                                {
                                                                                    x.AchievementId,
                                                                                    x.Flag
                                                                                });

                                                          table.ForeignKey(name: "FK_GuildWarsAchievementFlags_GuildWarsAchievements_AchievementId",
                                                                           column: x => x.AchievementId,
                                                                           principalTable: "GuildWarsAchievements",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable(name: "GuildWarsAchievementPrerequisites",
                                         columns: table => new
                                                           {
                                                               AchievementId = table.Column<int>(type: "int", nullable: false),
                                                               Id = table.Column<int>(type: "int", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildWarsAchievementPrerequisites",
                                                                           x => new
                                                                                {
                                                                                    x.AchievementId,
                                                                                    x.Id
                                                                                });

                                                          table.ForeignKey(name: "FK_GuildWarsAchievementPrerequisites_GuildWarsAchievements_AchievementId",
                                                                           column: x => x.AchievementId,
                                                                           principalTable: "GuildWarsAchievements",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable(name: "GuildWarsAchievementRewards",
                                         columns: table => new
                                                           {
                                                               AchievementId = table.Column<int>(type: "int", nullable: false),
                                                               Counter = table.Column<int>(type: "int", nullable: false),
                                                               Id = table.Column<int>(type: "int", nullable: false),
                                                               Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                               Count = table.Column<int>(type: "int", nullable: false),
                                                               Region = table.Column<string>(type: "nvarchar(max)", nullable: true)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildWarsAchievementRewards",
                                                                           x => new
                                                                                {
                                                                                    x.AchievementId,
                                                                                    x.Counter
                                                                                });

                                                          table.ForeignKey(name: "FK_GuildWarsAchievementRewards_GuildWarsAchievements_AchievementId",
                                                                           column: x => x.AchievementId,
                                                                           principalTable: "GuildWarsAchievements",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable(name: "GuildWarsAchievementTiers",
                                         columns: table => new
                                                           {
                                                               AchievementId = table.Column<int>(type: "int", nullable: false),
                                                               Counter = table.Column<int>(type: "int", nullable: false),
                                                               Count = table.Column<int>(type: "int", nullable: false),
                                                               Points = table.Column<int>(type: "int", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildWarsAchievementTiers",
                                                                           x => new
                                                                                {
                                                                                    x.AchievementId,
                                                                                    x.Counter
                                                                                });

                                                          table.ForeignKey(name: "FK_GuildWarsAchievementTiers_GuildWarsAchievements_AchievementId",
                                                                           column: x => x.AchievementId,
                                                                           principalTable: "GuildWarsAchievements",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GuildWarsAchievementBits");
            migrationBuilder.DropTable(name: "GuildWarsAchievementFlags");
            migrationBuilder.DropTable(name: "GuildWarsAchievementPrerequisites");
            migrationBuilder.DropTable(name: "GuildWarsAchievementRewards");
            migrationBuilder.DropTable(name: "GuildWarsAchievementTiers");
            migrationBuilder.DropTable(name: "GuildWarsAchievements");
        }
    }
}