using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 30
    /// </summary>
    public partial class Update30 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>("Points",
                                                 "RaidRegistrations",
                                                 "float",
                                                 nullable: true,
                                                 oldClrType: typeof(long),
                                                 oldType: "bigint",
                                                 oldNullable: true);
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>("Points",
                                               "RaidRegistrations",
                                               "bigint",
                                               nullable: true,
                                               oldClrType: typeof(double),
                                               oldType: "float",
                                               oldNullable: true);
        }
    }
}