
using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 32
    /// </summary>
    public partial class Update32 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>("ParticipationPoints",
                                                 "RaidExperienceLevels",
                                                 "float",
                                                 nullable: false,
                                                 oldClrType: typeof(long),
                                                 oldType: "bigint");

            migrationBuilder.AddColumn<DateTime>("TimeStamp",
                                                 "LogEntries",
                                                 "datetime2",
                                                 nullable: false,
                                                 defaultValue: new DateTime(1,
                                                                            1,
                                                                            1,
                                                                            0,
                                                                            0,
                                                                            0,
                                                                            0,
                                                                            DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>("Type",
                                            "LogEntries",
                                            "int",
                                            nullable: false,
                                            defaultValue: 0);
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("TimeStamp", "LogEntries");
            migrationBuilder.DropColumn("Type", "LogEntries");

            migrationBuilder.AlterColumn<long>("ParticipationPoints",
                                               "RaidExperienceLevels",
                                               "bigint",
                                               nullable: false,
                                               oldClrType: typeof(double),
                                               oldType: "float");
        }
    }
}