using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 6
    /// </summary>
    public partial class Update6 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>("MainRoleId",
                                               "RaidRoles",
                                               "bigint",
                                               nullable: true,
                                               oldClrType: typeof(long),
                                               oldType: "bigint");
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>("MainRoleId",
                                               "RaidRoles",
                                               "bigint",
                                               nullable: false,
                                               defaultValue: 0L,
                                               oldClrType: typeof(long),
                                               oldType: "bigint",
                                               oldNullable: true);
        }
    }
}