using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 65
    /// </summary>
    public partial class Update65 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_RaidUserRoles_RaidRoles_MainRoleId", table: "RaidUserRoles");
            migrationBuilder.DropForeignKey(name: "FK_RaidUserRoles_RaidRoles_SubRoleId", table: "RaidUserRoles");
            migrationBuilder.DropPrimaryKey(name: "PK_RaidUserRoles", table: "RaidUserRoles");
            migrationBuilder.DropIndex(name: "IX_RaidUserRoles_SubRoleId", table: "RaidUserRoles");
            migrationBuilder.DropColumn(name: "SubRoleId", table: "RaidUserRoles");
            migrationBuilder.RenameColumn(name: "MainRoleId", table: "RaidUserRoles", newName: "RoleId");
            migrationBuilder.RenameIndex(name: "IX_RaidUserRoles_MainRoleId", table: "RaidUserRoles", newName: "IX_RaidUserRoles_RoleId");
            migrationBuilder.RenameColumn(name: "Description", table: "RaidRoles", newName: "SelectMenuDescription");
            migrationBuilder.AddColumn<string>(name: "RegistrationDescription", table: "RaidRoles", type: "nvarchar(max)", nullable: true);

            migrationBuilder.AddPrimaryKey(name: "PK_RaidUserRoles",
                                           table: "RaidUserRoles",
                                           columns: new[]
                                                    {
                                                        "UserId",
                                                        "RoleId"
                                                    });

            migrationBuilder.AddForeignKey(name: "FK_RaidUserRoles_RaidRoles_RoleId",
                                           table: "RaidUserRoles",
                                           column: "RoleId",
                                           principalTable: "RaidRoles",
                                           principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_RaidUserRoles_RaidRoles_RoleId", table: "RaidUserRoles");
            migrationBuilder.DropPrimaryKey(name: "PK_RaidUserRoles", table: "RaidUserRoles");
            migrationBuilder.DropColumn(name: "RegistrationDescription", table: "RaidRoles");
            migrationBuilder.RenameColumn(name: "RoleId", table: "RaidUserRoles", newName: "SubRoleId");
            migrationBuilder.RenameIndex(name: "IX_RaidUserRoles_RoleId", table: "RaidUserRoles", newName: "IX_RaidUserRoles_SubRoleId");
            migrationBuilder.RenameColumn(name: "SelectMenuDescription", table: "RaidRoles", newName: "Description");

            migrationBuilder.AddColumn<long>(name: "MainRoleId",
                                             table: "RaidUserRoles",
                                             type: "bigint",
                                             nullable: false,
                                             defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(name: "PK_RaidUserRoles",
                                           table: "RaidUserRoles",
                                           columns: new[]
                                                    {
                                                        "UserId",
                                                        "MainRoleId",
                                                        "SubRoleId"
                                                    });
            migrationBuilder.CreateIndex(name: "IX_RaidUserRoles_MainRoleId", table: "RaidUserRoles", column: "MainRoleId");

            migrationBuilder.AddForeignKey(name: "FK_RaidUserRoles_RaidRoles_MainRoleId",
                                           table: "RaidUserRoles",
                                           column: "MainRoleId",
                                           principalTable: "RaidRoles",
                                           principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(name: "FK_RaidUserRoles_RaidRoles_SubRoleId",
                                           table: "RaidUserRoles",
                                           column: "SubRoleId",
                                           principalTable: "RaidRoles",
                                           principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);
        }
    }
}