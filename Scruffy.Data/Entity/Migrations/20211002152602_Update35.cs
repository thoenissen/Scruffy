using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 35
    /// </summary>
    public partial class Update35 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "QualifiedCommandName", table: "LogEntries", newName: "Source");
            migrationBuilder.RenameColumn(name: "LastUserCommand", table: "LogEntries", newName: "SubSource");
            migrationBuilder.AddColumn<string>(name: "AdditionalInformation", table: "LogEntries", type: "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<int>(name: "Level", table: "LogEntries", type: "int", nullable: false, defaultValue: 0);
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AdditionalInformation", table: "LogEntries");
            migrationBuilder.DropColumn(name: "Level", table: "LogEntries");
            migrationBuilder.RenameColumn(name: "SubSource", table: "LogEntries", newName: "QualifiedCommandName");
            migrationBuilder.RenameColumn(name: "Source", table: "LogEntries", newName: "LastUserCommand");
        }
    }
}