using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 11
    /// </summary>
    public partial class Update11 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>("SuperiorExperienceLevelId",
                                               "RaidExperienceLevels",
                                               "bigint",
                                               nullable: true,
                                               oldClrType: typeof(long),
                                               oldType: "bigint");
            migrationBuilder.AddColumn<string>("Description", "RaidExperienceLevels", "nvarchar(max)", nullable: true);

            migrationBuilder.AddColumn<decimal>("DiscordEmoji",
                                                "RaidExperienceLevels",
                                                "decimal(20,0)",
                                                nullable: false,
                                                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>("IsDeleted",
                                             "RaidExperienceLevels",
                                             "bit",
                                             nullable: false,
                                             defaultValue: false);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("Description", "RaidExperienceLevels");
            migrationBuilder.DropColumn("DiscordEmoji", "RaidExperienceLevels");
            migrationBuilder.DropColumn("IsDeleted", "RaidExperienceLevels");

            migrationBuilder.AlterColumn<long>("SuperiorExperienceLevelId",
                                               "RaidExperienceLevels",
                                               "bigint",
                                               nullable: false,
                                               defaultValue: 0L,
                                               oldClrType: typeof(long),
                                               oldType: "bigint",
                                               oldNullable: true);
        }
    }
}