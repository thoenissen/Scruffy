using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations;

/// <inheritdoc />
public partial class Update85 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("DpsReports",
                                     columns: table => new
                                     {
                                         UserId = table.Column<long>("bigint", nullable: false),
                                         Id = table.Column<string>("nvarchar(64)", maxLength: 64, nullable: false),
                                         PermaLink = table.Column<string>("nvarchar(64)", maxLength: 64, nullable: true),
                                         UploadTime = table.Column<DateTime>("datetime2", nullable: false),
                                         EncounterTime = table.Column<DateTime>("datetime2", nullable: false),
                                         BossId = table.Column<long>("bigint", nullable: false),
                                         IsSuccess = table.Column<bool>("bit", nullable: false),
                                         Mode = table.Column<int>("int", nullable: false),
                                         State = table.Column<int>("int", nullable: false)
                                     },
                                     constraints: table =>
                                     {
                                         table.PrimaryKey("PK_DpsReports", x => new { x.UserId, x.Id });
                                         table.ForeignKey(
                                             "FK_DpsReports_Users_UserId",
                                             column: x => x.UserId,
                                             principalTable: "Users",
                                             principalColumn: "Id",
                                             onDelete: ReferentialAction.Restrict);
                                     });

        migrationBuilder.CreateTable("UserDpsReportsConfigurations",
                                     columns: table => new
                                     {
                                         UserId = table.Column<long>("bigint", nullable: false),
                                         UserToken = table.Column<string>("nvarchar(64)", maxLength: 64, nullable: true),
                                         LastImport = table.Column<DateTime>("datetime2", nullable: true),
                                         IsImportActivated = table.Column<bool>("bit", nullable: false)
                                     },
                                     constraints: table =>
                                     {
                                         table.PrimaryKey("PK_UserDpsReportsConfigurations", x => x.UserId);
                                         table.ForeignKey(
                                             "FK_UserDpsReportsConfigurations_Users_UserId",
                                             column: x => x.UserId,
                                             principalTable: "Users",
                                             principalColumn: "Id",
                                             onDelete: ReferentialAction.Restrict);
                                     });

        migrationBuilder.CreateIndex("IX_UserId_EncounterTime", "DpsReports", ["UserId", "EncounterTime"]);
        migrationBuilder.CreateIndex("IX_UserId_UploadTime", "DpsReports", ["UserId", "UploadTime"]);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("DpsReports");
        migrationBuilder.DropTable("UserDpsReportsConfigurations");
    }
}