using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 67
    /// </summary>
    public partial class Update67 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(name: "AccessFailedCount",
                                            table: "Users",
                                            type: "int",
                                            nullable: false,
                                            defaultValue: 0);
            migrationBuilder.AddColumn<string>(name: "ConcurrencyStamp", table: "Users", type: "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Email",
                                               table: "Users",
                                               type: "nvarchar(256)",
                                               maxLength: 256,
                                               nullable: true);
            migrationBuilder.AddColumn<bool>(name: "EmailConfirmed",
                                             table: "Users",
                                             type: "bit",
                                             nullable: false,
                                             defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "LockoutEnabled",
                                             table: "Users",
                                             type: "bit",
                                             nullable: false,
                                             defaultValue: false);
            migrationBuilder.AddColumn<DateTimeOffset>(name: "LockoutEnd", table: "Users", type: "datetimeoffset", nullable: true);
            migrationBuilder.AddColumn<string>(name: "NormalizedEmail",
                                               table: "Users",
                                               type: "nvarchar(256)",
                                               maxLength: 256,
                                               nullable: true);

            migrationBuilder.AddColumn<string>(name: "NormalizedUserName",
                                               table: "Users",
                                               type: "nvarchar(256)",
                                               maxLength: 256,
                                               nullable: true);
            migrationBuilder.AddColumn<string>(name: "PasswordHash", table: "Users", type: "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "PhoneNumber", table: "Users", type: "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<bool>(name: "PhoneNumberConfirmed",
                                             table: "Users",
                                             type: "bit",
                                             nullable: false,
                                             defaultValue: false);
            migrationBuilder.AddColumn<string>(name: "SecurityStamp", table: "Users", type: "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<bool>(name: "TwoFactorEnabled",
                                             table: "Users",
                                             type: "bit",
                                             nullable: false,
                                             defaultValue: false);
            migrationBuilder.AddColumn<string>(name: "UserName",
                                               table: "Users",
                                               type: "nvarchar(256)",
                                               maxLength: 256,
                                               nullable: true);
            migrationBuilder.CreateTable(name: "Roles",
                                         columns: table => new
                                                           {
                                                               Id = table.Column<long>(type: "bigint", nullable: false)
                                                                         .Annotation("SqlServer:Identity", "1, 1"),
                                                               Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                                                               NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                                                               ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                                                           },
                                         constraints: table => table.PrimaryKey("PK_Roles", x => x.Id));
            migrationBuilder.CreateTable(name: "UserClaims",
                                         columns: table => new
                                                           {
                                                               Id = table.Column<int>(type: "int", nullable: false)
                                                                         .Annotation("SqlServer:Identity", "1, 1"),
                                                               UserId = table.Column<long>(type: "bigint", nullable: false),
                                                               ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                               ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_UserClaims", x => x.Id);

                                                          table.ForeignKey(name: "FK_UserClaims_Users_UserId",
                                                                           column: x => x.UserId,
                                                                           principalTable: "Users",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
            migrationBuilder.CreateTable(name: "UserLogins",
                                         columns: table => new
                                                           {
                                                               LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                                                               ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                                                               ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                               UserId = table.Column<long>(type: "bigint", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_UserLogins",
                                                                           x => new
                                                                                {
                                                                                    x.LoginProvider,
                                                                                    x.ProviderKey
                                                                                });

                                                          table.ForeignKey(name: "FK_UserLogins_Users_UserId",
                                                                           column: x => x.UserId,
                                                                           principalTable: "Users",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
            migrationBuilder.CreateTable(name: "UserTokens",
                                         columns: table => new
                                                           {
                                                               UserId = table.Column<long>(type: "bigint", nullable: false),
                                                               LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                                                               Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                                                               Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_UserTokens",
                                                                           x => new
                                                                                {
                                                                                    x.UserId,
                                                                                    x.LoginProvider,
                                                                                    x.Name
                                                                                });

                                                          table.ForeignKey(name: "FK_UserTokens_Users_UserId",
                                                                           column: x => x.UserId,
                                                                           principalTable: "Users",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
            migrationBuilder.CreateTable(name: "RoleClaims",
                                         columns: table => new
                                                           {
                                                               Id = table.Column<int>(type: "int", nullable: false)
                                                                         .Annotation("SqlServer:Identity", "1, 1"),
                                                               RoleId = table.Column<long>(type: "bigint", nullable: false),
                                                               ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                               ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_RoleClaims", x => x.Id);

                                                          table.ForeignKey(name: "FK_RoleClaims_Roles_RoleId",
                                                                           column: x => x.RoleId,
                                                                           principalTable: "Roles",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
            migrationBuilder.CreateTable(name: "UserRoles",
                                         columns: table => new
                                                           {
                                                               UserId = table.Column<long>(type: "bigint", nullable: false),
                                                               RoleId = table.Column<long>(type: "bigint", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_UserRoles",
                                                                           x => new
                                                                                {
                                                                                    x.UserId,
                                                                                    x.RoleId
                                                                                });

                                                          table.ForeignKey(name: "FK_UserRoles_Roles_RoleId",
                                                                           column: x => x.RoleId,
                                                                           principalTable: "Roles",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey(name: "FK_UserRoles_Users_UserId",
                                                                           column: x => x.UserId,
                                                                           principalTable: "Users",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
            migrationBuilder.CreateIndex(name: "EmailIndex", table: "Users", column: "NormalizedEmail");
            migrationBuilder.CreateIndex(name: "UserNameIndex",
                                         table: "Users",
                                         column: "NormalizedUserName",
                                         unique: true,
                                         filter: "[NormalizedUserName] IS NOT NULL");
            migrationBuilder.CreateIndex(name: "IX_RoleClaims_RoleId", table: "RoleClaims", column: "RoleId");
            migrationBuilder.CreateIndex(name: "RoleNameIndex",
                                         table: "Roles",
                                         column: "NormalizedName",
                                         unique: true,
                                         filter: "[NormalizedName] IS NOT NULL");
            migrationBuilder.CreateIndex(name: "IX_UserClaims_UserId", table: "UserClaims", column: "UserId");
            migrationBuilder.CreateIndex(name: "IX_UserLogins_UserId", table: "UserLogins", column: "UserId");
            migrationBuilder.CreateIndex(name: "IX_UserRoles_RoleId", table: "UserRoles", column: "RoleId");

            migrationBuilder.Sql(@"SET IDENTITY_INSERT [ROLES] ON
                                   
                                   INSERT INTO [Roles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
                                                VALUES (1, 'Developer', 'DEVELOPER', NEWID());
                                   
                                   INSERT INTO [Roles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
                                                VALUES (2, 'Administrator', 'ADMINISTRATOR', NEWID());
                                   
                                   SET IDENTITY_INSERT [ROLES] ON");
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "RoleClaims");
            migrationBuilder.DropTable(name: "UserClaims");
            migrationBuilder.DropTable(name: "UserLogins");
            migrationBuilder.DropTable(name: "UserRoles");
            migrationBuilder.DropTable(name: "UserTokens");
            migrationBuilder.DropTable(name: "Roles");
            migrationBuilder.DropIndex(name: "EmailIndex", table: "Users");
            migrationBuilder.DropIndex(name: "UserNameIndex", table: "Users");
            migrationBuilder.DropColumn(name: "AccessFailedCount", table: "Users");
            migrationBuilder.DropColumn(name: "ConcurrencyStamp", table: "Users");
            migrationBuilder.DropColumn(name: "Email", table: "Users");
            migrationBuilder.DropColumn(name: "EmailConfirmed", table: "Users");
            migrationBuilder.DropColumn(name: "LockoutEnabled", table: "Users");
            migrationBuilder.DropColumn(name: "LockoutEnd", table: "Users");
            migrationBuilder.DropColumn(name: "NormalizedEmail", table: "Users");
            migrationBuilder.DropColumn(name: "NormalizedUserName", table: "Users");
            migrationBuilder.DropColumn(name: "PasswordHash", table: "Users");
            migrationBuilder.DropColumn(name: "PhoneNumber", table: "Users");
            migrationBuilder.DropColumn(name: "PhoneNumberConfirmed", table: "Users");
            migrationBuilder.DropColumn(name: "SecurityStamp", table: "Users");
            migrationBuilder.DropColumn(name: "TwoFactorEnabled", table: "Users");
            migrationBuilder.DropColumn(name: "UserName", table: "Users");
        }
    }
}