using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations;

/// <summary>
/// Update 64
/// </summary>
public partial class Update64 : Migration
{
    #region Migration

    /// <inheritdoc/>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "GuildDiscordRoles",
                                     columns: table => new
                                                       {
                                                           GuildId = table.Column<long>(type: "bigint", nullable: false),
                                                           RoleId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                           Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true)
                                                       },
                                     constraints: table =>
                                                  {
                                                      table.PrimaryKey("PK_GuildDiscordRoles",
                                                                       column => new
                                                                                 {
                                                                                     column.GuildId,
                                                                                     column.RoleId
                                                                                 });

                                                      table.ForeignKey(name: "FK_GuildDiscordRoles_Guilds_GuildId",
                                                                       column: column => column.GuildId,
                                                                       principalTable: "Guilds",
                                                                       principalColumn: "Id",
                                                                       onDelete: ReferentialAction.Restrict);
                                                  });
    }

    /// <inheritdoc/>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "GuildDiscordRoles");
    }

    #endregion // Migration
}