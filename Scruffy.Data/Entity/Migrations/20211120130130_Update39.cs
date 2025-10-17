using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 39
    /// </summary>
    public partial class Update39 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(name: "GuildWarsAccountAchievements",
                                         columns: table => new
                                                           {
                                                               AccountName = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                                                               AchievementId = table.Column<int>(type: "int", nullable: false),
                                                               Current = table.Column<int>(type: "int", nullable: true),
                                                               Maximum = table.Column<int>(type: "int", nullable: true),
                                                               IsDone = table.Column<bool>(type: "bit", nullable: false),
                                                               RepetitionCount = table.Column<int>(type: "int", nullable: true),
                                                               IsUnlocked = table.Column<bool>(type: "bit", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildWarsAccountAchievements",
                                                                           x => new
                                                                                {
                                                                                    x.AccountName,
                                                                                    x.AchievementId
                                                                                });

                                                          table.ForeignKey(name: "FK_GuildWarsAccountAchievements_GuildWarsAccounts_AccountName",
                                                                           column: x => x.AccountName,
                                                                           principalTable: "GuildWarsAccounts",
                                                                           principalColumn: "Name",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable(name: "GuildWarsAccountAchievementBits",
                                         columns: table => new
                                                           {
                                                               AccountName = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                                                               AchievementId = table.Column<int>(type: "int", nullable: false),
                                                               Bit = table.Column<int>(type: "int", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_GuildWarsAccountAchievementBits",
                                                                           x => new
                                                                                {
                                                                                    x.AccountName,
                                                                                    x.AchievementId,
                                                                                    x.Bit
                                                                                });

                                                          table.ForeignKey(name: "FK_GuildWarsAccountAchievementBits_GuildWarsAccountAchievements_AccountName_AchievementId",
                                                                           columns: x => new
                                                                                         {
                                                                                             x.AccountName,
                                                                                             x.AchievementId
                                                                                         },
                                                                           principalTable: "GuildWarsAccountAchievements",
                                                                           principalColumns: ["AccountName", "AchievementId"],
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey(name: "FK_GuildWarsAccountAchievementBits_GuildWarsAccounts_AccountName",
                                                                           column: x => x.AccountName,
                                                                           principalTable: "GuildWarsAccounts",
                                                                           principalColumn: "Name",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GuildWarsAccountAchievementBits");
            migrationBuilder.DropTable(name: "GuildWarsAccountAchievements");
        }
    }
}