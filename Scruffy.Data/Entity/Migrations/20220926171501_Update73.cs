using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations;

/// <summary>
/// Update 73
/// </summary>
public partial class Update73 : Migration
{
    #region Migration

    /// <inheritdoc/>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "GuildRankNotifications",
                                     columns: table => new
                                                       {
                                                           GuildId = table.Column<long>(type: "bigint", nullable: false),
                                                           UserId = table.Column<long>(type: "bigint", nullable: false),
                                                           Type = table.Column<int>(type: "int", nullable: false),
                                                           RoleId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                                                       },
                                     constraints: table =>
                                                  {
                                                      table.PrimaryKey("PK_GuildRankNotifications",
                                                                       column => new
                                                                                 {
                                                                                     column.GuildId,
                                                                                     column.UserId,
                                                                                     column.Type
                                                                                 });

                                                      table.ForeignKey(name: "FK_GuildRankNotifications_Guilds_GuildId",
                                                                       column: column => column.GuildId,
                                                                       principalTable: "Guilds",
                                                                       principalColumn: "Id",
                                                                       onDelete: ReferentialAction.Restrict);

                                                      table.ForeignKey(name: "FK_GuildRankNotifications_Users_UserId",
                                                                       column: column => column.UserId,
                                                                       principalTable: "Users",
                                                                       principalColumn: "Id",
                                                                       onDelete: ReferentialAction.Restrict);
                                                  });
        migrationBuilder.CreateIndex(name: "IX_GuildRankNotifications_UserId", table: "GuildRankNotifications", column: "UserId");
    }

    /// <inheritdoc/>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "GuildRankNotifications");
    }

    #endregion // Migration
}