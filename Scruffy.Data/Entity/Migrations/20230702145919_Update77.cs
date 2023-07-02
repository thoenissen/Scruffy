using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <inheritdoc/>
    public partial class Update77 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GameChannels");
            migrationBuilder.DropTable(name: "LookingForGroupParticipants");
            migrationBuilder.DropTable(name: "LookingForGroupAppointments");
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(name: "GameChannels",
                                         columns: table => new
                                                           {
                                                               Id = table.Column<long>(type: "bigint", nullable: false)
                                                                         .Annotation("SqlServer:Identity", "1, 1"),
                                                               DiscordChannelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               Type = table.Column<int>(type: "int", nullable: false)
                                                           },
                                         constraints: table => table.PrimaryKey("PK_GameChannels", x => x.Id));

            migrationBuilder.CreateTable(name: "LookingForGroupAppointments",
                                         columns: table => new
                                                           {
                                                               Id = table.Column<int>(type: "int", nullable: false)
                                                                         .Annotation("SqlServer:Identity", "1, 1"),
                                                               CreationUserId = table.Column<long>(type: "bigint", nullable: false),
                                                               ChannelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                               MessageId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               ThreadId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_LookingForGroupAppointments", x => x.Id);

                                                          table.ForeignKey(name: "FK_LookingForGroupAppointments_Users_CreationUserId",
                                                                           column: x => x.CreationUserId,
                                                                           principalTable: "Users",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable(name: "LookingForGroupParticipants",
                                         columns: table => new
                                                           {
                                                               AppointmentId = table.Column<int>(type: "int", nullable: false),
                                                               UserId = table.Column<long>(type: "bigint", nullable: false),
                                                               RegistrationTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_LookingForGroupParticipants",
                                                                           x => new
                                                                                {
                                                                                    x.AppointmentId,
                                                                                    x.UserId
                                                                                });

                                                          table.ForeignKey(name: "FK_LookingForGroupParticipants_LookingForGroupAppointments_AppointmentId",
                                                                           column: x => x.AppointmentId,
                                                                           principalTable: "LookingForGroupAppointments",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);

                                                          table.ForeignKey(name: "FK_LookingForGroupParticipants_Users_UserId",
                                                                           column: x => x.UserId,
                                                                           principalTable: "Users",
                                                                           principalColumn: "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
            migrationBuilder.CreateIndex(name: "IX_LookingForGroupAppointments_CreationUserId", table: "LookingForGroupAppointments", column: "CreationUserId");
            migrationBuilder.CreateIndex(name: "IX_LookingForGroupParticipants_UserId", table: "LookingForGroupParticipants", column: "UserId");
        }
    }
}