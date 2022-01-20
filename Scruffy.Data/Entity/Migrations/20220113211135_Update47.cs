using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 47
    /// </summary>
    public partial class Update47 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(name: "PK_GuildRankCurrentPointsEntity", table: "GuildRankCurrentPointsEntity");

            migrationBuilder.AddColumn<DateTime>(name: "Date",
                                                 table: "GuildRankCurrentPointsEntity",
                                                 type: "datetime2",
                                                 nullable: false,
                                                 defaultValue: new DateTime(1,
                                                                            1,
                                                                            1,
                                                                            0,
                                                                            0,
                                                                            0,
                                                                            0,
                                                                            DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(name: "PK_GuildRankCurrentPointsEntity",
                                           table: "GuildRankCurrentPointsEntity",
                                           columns: new[]
                                                    {
                                                        "GuildId",
                                                        "UserId",
                                                        "Date",
                                                        "Type"
                                                    });
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(name: "PK_GuildRankCurrentPointsEntity", table: "GuildRankCurrentPointsEntity");
            migrationBuilder.DropColumn(name: "Date", table: "GuildRankCurrentPointsEntity");

            migrationBuilder.AddPrimaryKey(name: "PK_GuildRankCurrentPointsEntity",
                                           table: "GuildRankCurrentPointsEntity",
                                           columns: new[]
                                                    {
                                                        "GuildId",
                                                        "UserId",
                                                        "Type"
                                                    });
        }
    }
}