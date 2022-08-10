using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 66
    /// </summary>
    public partial class Update66 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_RaidRegistrationRoleAssignments_RaidRoles_MainRoleId", table: "RaidRegistrationRoleAssignments");
            migrationBuilder.DropForeignKey(name: "FK_RaidRegistrationRoleAssignments_RaidRoles_SubRoleId", table: "RaidRegistrationRoleAssignments");
            migrationBuilder.DropForeignKey(name: "FK_RaidRoles_RaidRoles_MainRoleId", table: "RaidRoles");

            migrationBuilder.DropTable(name: "RaidRequiredRoles");
            migrationBuilder.DropTable(name: "RaidRoleAliasNames");
            migrationBuilder.DropTable(name: "RaidRoleLineupAssignments");
            migrationBuilder.DropTable(name: "RaidRoleLineupEntries");
            migrationBuilder.DropTable(name: "RaidRoleLineupHeaders");

            migrationBuilder.DropIndex(name: "IX_RaidRoles_MainRoleId", table: "RaidRoles");
            migrationBuilder.DropIndex(name: "IX_RaidRegistrationRoleAssignments_MainRoleId", table: "RaidRegistrationRoleAssignments");
            migrationBuilder.DropIndex(name: "IX_RaidRegistrationRoleAssignments_SubRoleId", table: "RaidRegistrationRoleAssignments");

            migrationBuilder.Sql("DELETE FROM [RaidRegistrationRoleAssignments]");
            migrationBuilder.Sql("DELETE FROM [RaidUserRoles]");
            migrationBuilder.Sql("DELETE FROM [RaidRoles]");

            migrationBuilder.DropColumn(name: "MainRoleId", table: "RaidRoles");
            migrationBuilder.DropColumn(name: "RegistrationDescription", table: "RaidRoles");
            migrationBuilder.DropColumn(name: "SelectMenuDescription", table: "RaidRoles");
            migrationBuilder.DropColumn(name: "MainRoleId", table: "RaidRegistrationRoleAssignments");
            migrationBuilder.DropColumn(name: "SubRoleId", table: "RaidRegistrationRoleAssignments");
            migrationBuilder.DropColumn(name: "IsDeleted", table: "RaidRoles");

            migrationBuilder.AddColumn<bool>(name: "IsTank",
                                             table: "RaidRoles",
                                             type: "bit",
                                             nullable: false,
                                             defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "IsDamageDealer",
                                             table: "RaidRoles",
                                             type: "bit",
                                             nullable: false,
                                             defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "IsHealer",
                                             table: "RaidRoles",
                                             type: "bit",
                                             nullable: false,
                                             defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "IsProvidingAlacrity",
                                             table: "RaidRoles",
                                             type: "bit",
                                             nullable: false,
                                             defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "IsProvidingQuickness",
                                             table: "RaidRoles",
                                             type: "bit",
                                             nullable: false,
                                             defaultValue: false);
            migrationBuilder.AddColumn<long>(name: "RoleId",
                                             table: "RaidRegistrationRoleAssignments",
                                             type: "bigint",
                                             nullable: false,
                                             defaultValue: 0L);
            migrationBuilder.CreateIndex(name: "IX_RaidRegistrationRoleAssignments_RoleId", table: "RaidRegistrationRoleAssignments", column: "RoleId");
            migrationBuilder.AddForeignKey(name: "FK_RaidRegistrationRoleAssignments_RaidRoles_RoleId",
                                           table: "RaidRegistrationRoleAssignments",
                                           column: "RoleId",
                                           principalTable: "RaidRoles",
                                           principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"SET IDENTITY_INSERT [RaidRoles] ON;
                                   INSERT INTO [RaidRoles] ([Id], [DiscordEmojiId], [IsTank], [IsDamageDealer], [IsHealer], [IsProvidingAlacrity], [IsProvidingQuickness]) VALUES (1, 744104683081171004, 0, 1, 0, 0, 0)
                                   INSERT INTO [RaidRoles] ([Id], [DiscordEmojiId], [IsTank], [IsDamageDealer], [IsHealer], [IsProvidingAlacrity], [IsProvidingQuickness]) VALUES (2, 994543559027986442, 0, 1, 0, 1, 0)
                                   INSERT INTO [RaidRoles] ([Id], [DiscordEmojiId], [IsTank], [IsDamageDealer], [IsHealer], [IsProvidingAlacrity], [IsProvidingQuickness]) VALUES (3, 994543518972391475, 0, 1, 0, 0, 1)
                                   INSERT INTO [RaidRoles] ([Id], [DiscordEmojiId], [IsTank], [IsDamageDealer], [IsHealer], [IsProvidingAlacrity], [IsProvidingQuickness]) VALUES (4, 994543591525462036, 0, 0, 1, 1, 0)
                                   INSERT INTO [RaidRoles] ([Id], [DiscordEmojiId], [IsTank], [IsDamageDealer], [IsHealer], [IsProvidingAlacrity], [IsProvidingQuickness]) VALUES (5, 994543468758171708, 0, 0, 1, 0, 1)
                                   INSERT INTO [RaidRoles] ([Id], [DiscordEmojiId], [IsTank], [IsDamageDealer], [IsHealer], [IsProvidingAlacrity], [IsProvidingQuickness]) VALUES (6, 1006997115395915806, 1, 1, 0, 1, 0)
                                   INSERT INTO [RaidRoles] ([Id], [DiscordEmojiId], [IsTank], [IsDamageDealer], [IsHealer], [IsProvidingAlacrity], [IsProvidingQuickness]) VALUES (7, 1006997118411620392, 1, 1, 0, 0, 1)
                                   INSERT INTO [RaidRoles] ([Id], [DiscordEmojiId], [IsTank], [IsDamageDealer], [IsHealer], [IsProvidingAlacrity], [IsProvidingQuickness]) VALUES (8, 1006997117086216233, 1, 0, 1, 1, 0)
                                   INSERT INTO [RaidRoles] ([Id], [DiscordEmojiId], [IsTank], [IsDamageDealer], [IsHealer], [IsProvidingAlacrity], [IsProvidingQuickness]) VALUES (9, 1006997252415426762, 1, 0, 1, 0, 1)
                                   SET IDENTITY_INSERT [RaidRoles] OFF;
                                   DBCC CHECKIDENT ('[RaidRoles]', RESEED, 0);");
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException();
        }
    }
}