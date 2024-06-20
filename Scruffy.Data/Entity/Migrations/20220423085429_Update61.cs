using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 61
    /// </summary>
    public partial class Update61 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DpsReportUserToken", table: "GuildWarsAccounts");
            migrationBuilder.AddColumn<DateTime>(name: "Birthday", table: "Users", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<string>(name: "DpsReportUserToken", table: "Users", type: "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<bool>(name: "IsDataStorageAccepted", table: "Users", type: "bit", nullable: true);
            migrationBuilder.AddColumn<bool>(name: "IsExtendedDataStorageAccepted", table: "Users", type: "bit", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Name", table: "Users", type: "nvarchar(max)", nullable: true);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Birthday", table: "Users");
            migrationBuilder.DropColumn(name: "DpsReportUserToken", table: "Users");
            migrationBuilder.DropColumn(name: "IsDataStorageAccepted", table: "Users");
            migrationBuilder.DropColumn(name: "IsExtendedDataStorageAccepted", table: "Users");
            migrationBuilder.DropColumn(name: "Name", table: "Users");
            migrationBuilder.AddColumn<string>(name: "DpsReportUserToken", table: "GuildWarsAccounts", type: "nvarchar(max)", nullable: true);
        }
    }
}