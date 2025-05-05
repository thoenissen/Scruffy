using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <inheritdoc />
    public partial class Update80 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("LookingForGroupAppointments",
                                         columns: table => new
                                                           {
                                                               Id = table.Column<int>("int", nullable: false)
                                                                         .Annotation("SqlServer:Identity", "1, 1"),
                                                               Title = table.Column<string>("nvarchar(max)", nullable: true),
                                                               Description = table.Column<string>("nvarchar(max)", nullable: true),
                                                               CreationUserId = table.Column<long>("bigint", nullable: false),
                                                               ChannelId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                               MessageId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                               ThreadId = table.Column<decimal>("decimal(20,0)", nullable: false)
                                                           },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_LookingForGroupAppointments", x => x.Id);
                                                          table.ForeignKey(
                                                              "FK_LookingForGroupAppointments_Users_CreationUserId",
                                                              column: x => x.CreationUserId,
                                                              principalTable: "Users",
                                                              principalColumn: "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                                      });

            migrationBuilder.CreateTable("LookingForGroupParticipants",
                                         columns: table => new
                                                           {
                                                               AppointmentId = table.Column<int>("int", nullable: false),
                                                               UserId = table.Column<long>("bigint", nullable: false),
                                                               RegistrationTimeStamp = table.Column<DateTime>("datetime2", nullable: false)
                                                           },
                                         constraints: table =>
                                         {
                                             table.PrimaryKey("PK_LookingForGroupParticipants", x => new { x.AppointmentId, x.UserId });
                                             table.ForeignKey("FK_LookingForGroupParticipants_LookingForGroupAppointments_AppointmentId",
                                                              column: x => x.AppointmentId,
                                                              principalTable: "LookingForGroupAppointments",
                                                              principalColumn: "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                             table.ForeignKey("FK_LookingForGroupParticipants_Users_UserId",
                                                              column: x => x.UserId,
                                                              principalTable: "Users",
                                                              principalColumn: "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });

            migrationBuilder.CreateIndex("IX_LookingForGroupAppointments_CreationUserId",
                                         "LookingForGroupAppointments",
                                         "CreationUserId");

            migrationBuilder.CreateIndex("IX_LookingForGroupParticipants_UserId",
                                         "LookingForGroupParticipants",
                                         "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("LookingForGroupParticipants");
            migrationBuilder.DropTable("LookingForGroupAppointments");
        }
    }
}