using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 62
    /// </summary>
    public partial class Update62 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "FractalRegistrations");
            migrationBuilder.DropTable(name: "FractalAppointments");
            migrationBuilder.DropTable(name: "FractalLfgConfigurations");
            migrationBuilder.AddColumn<string>(name: "WelcomeDirectMessage", table: "Guilds", type: "nvarchar(max)", nullable: true);
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "WelcomeDirectMessage", table: "Guilds");

            migrationBuilder.CreateTable(name: "FractalLfgConfigurations",
                                         columns: table => new
                                                           {
                                                               Id = table.Column<long>(type: "bigint", nullable: false)
                                                                         .Annotation("SqlServer:Identity", "1, 1"),
                                                               AliasName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                                                               Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                               DiscordChannelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               DiscordMessageId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
                                                           },
                                         constraints: table => table.PrimaryKey("PK_FractalLfgConfigurations", x => x.Id));

            migrationBuilder.CreateTable(name: "FractalAppointments",
                                         columns: table => new
                                                           {
                                                               Id = table.Column<long>(type: "bigint", nullable: false)
                                                                         .Annotation("SqlServer:Identity", "1, 1"),
                                                               ConfigurationId = table.Column<long>(type: "bigint", nullable: false),
                                                               AppointmentTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                                                               DiscordMessageId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_FractalAppointments", x => x.Id);

                                                          table.ForeignKey(name: "FK_FractalAppointments_FractalLfgConfigurations_ConfigurationId",
                                                                           column: x => x.ConfigurationId,
                                                                           principalTable: "FractalLfgConfigurations",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable(name: "FractalRegistrations",
                                         columns: table => new
                                                           {
                                                               ConfigurationId = table.Column<long>(type: "bigint", nullable: false),
                                                               AppointmentTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                                                               UserId = table.Column<long>(type: "bigint", nullable: false),
                                                               AppointmentId = table.Column<long>(type: "bigint", nullable: true),
                                                               RegistrationTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_FractalRegistrations",
                                                                           x => new
                                                                                {
                                                                                    x.ConfigurationId,
                                                                                    x.AppointmentTimeStamp,
                                                                                    x.UserId
                                                                                });
                                                          table.ForeignKey(name: "FK_FractalRegistrations_FractalAppointments_AppointmentId", column: x => x.AppointmentId, principalTable: "FractalAppointments", principalColumn: "Id");

                                                          table.ForeignKey(name: "FK_FractalRegistrations_FractalLfgConfigurations_ConfigurationId",
                                                                           column: x => x.ConfigurationId,
                                                                           principalTable: "FractalLfgConfigurations",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey(name: "FK_FractalRegistrations_Users_UserId",
                                                                           column: x => x.UserId,
                                                                           principalTable: "Users",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
            migrationBuilder.CreateIndex(name: "IX_FractalAppointments_ConfigurationId", table: "FractalAppointments", column: "ConfigurationId");
            migrationBuilder.CreateIndex(name: "IX_FractalRegistrations_AppointmentId", table: "FractalRegistrations", column: "AppointmentId");
            migrationBuilder.CreateIndex(name: "IX_FractalRegistrations_UserId", table: "FractalRegistrations", column: "UserId");
        }
    }
}