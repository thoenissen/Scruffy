using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 53
    /// </summary>
    public partial class Update53 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey("PK_GuildWarsAccountHistoricCharacters",
                                            "GuildWarsAccountHistoricCharacters");

            migrationBuilder.AlterColumn<string>(name: "CharacterName",
                                                 table: "GuildWarsAccountHistoricCharacters",
                                                 type: "nvarchar(20)",
                                                 maxLength: 20,
                                                 nullable: false,
                                                 oldClrType: typeof(string),
                                                 oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey("PK_GuildWarsAccountHistoricCharacters",
                                           "GuildWarsAccountHistoricCharacters",
                                           new string[] { "Date", "AccountName", "CharacterName" });
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey("PK_GuildWarsAccountHistoricCharacters",
                                            "GuildWarsAccountHistoricCharacters");

            migrationBuilder.AlterColumn<string>(name: "CharacterName",
                                                 table: "GuildWarsAccountHistoricCharacters",
                                                 type: "nvarchar(450)",
                                                 nullable: false,
                                                 oldClrType: typeof(string),
                                                 oldType: "nvarchar(20)",
                                                 oldMaxLength: 20);

            migrationBuilder.AddPrimaryKey("PK_GuildWarsAccountHistoricCharacters",
                                           "GuildWarsAccountHistoricCharacters",
                                           new string[] { "Date", "AccountName", "CharacterName" });
        }
    }
}