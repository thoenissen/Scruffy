using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 25
    /// </summary>
    public partial class Update25 : Migration
    {
        /// <inheritdoc/>
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

        /// <inheritdoc/>
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