using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 25
    /// </summary>
    public partial class Update25 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>("Amount",
                                                 "GuildSpecialRankProtocolEntries",
                                                 "float",
                                                 nullable: true,
                                                 oldClrType: typeof(decimal),
                                                 oldType: "decimal(20,0)",
                                                 oldNullable: true);
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>("Amount",
                                                  "GuildSpecialRankProtocolEntries",
                                                  "decimal(20,0)",
                                                  nullable: true,
                                                  oldClrType: typeof(double),
                                                  oldType: "float",
                                                  oldNullable: true);
        }
    }
}