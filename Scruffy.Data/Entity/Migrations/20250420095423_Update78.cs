using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations;

/// <inheritdoc />
public partial class Update78 : Migration
{
    #region Migration

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("RaidAppointmentLineUpSquads",
                                     table => new
                                              {
                                                  AppointmentId = table.Column<long>("bigint", nullable: false),
                                                  GroupNumber = table.Column<int>("int", nullable: false),
                                                  MessageId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                  TankUserId = table.Column<long>("bigint", nullable: true),
                                                  Support1UserId = table.Column<long>("bigint", nullable: true),
                                                  Dps1UserId = table.Column<long>("bigint", nullable: true),
                                                  Dps2UserId = table.Column<long>("bigint", nullable: true),
                                                  Dps3UserId = table.Column<long>("bigint", nullable: true),
                                                  HealerUserId = table.Column<long>("bigint", nullable: true),
                                                  Support2UserId = table.Column<long>("bigint", nullable: true),
                                                  Dps4UserId = table.Column<long>("bigint", nullable: true),
                                                  Dps5UserId = table.Column<long>("bigint", nullable: true),
                                                  Dps6UserId = table.Column<long>("bigint", nullable: true)
                                              },
                                     constraints: table =>
                                                  {
                                                      table.PrimaryKey("PK_RaidAppointmentLineUpSquads",
                                                                       column => new
                                                                                 {
                                                                                     column.AppointmentId,
                                                                                     column.GroupNumber
                                                                                 });
                                                      table.ForeignKey("FK_RaidAppointmentLineUpSquads_RaidAppointments_AppointmentId", column => column.AppointmentId, "RaidAppointments", "Id", onDelete: ReferentialAction.Restrict);
                                                      table.ForeignKey("FK_RaidAppointmentLineUpSquads_Users_Dps1UserId", column => column.Dps1UserId, "Users", "Id");
                                                      table.ForeignKey("FK_RaidAppointmentLineUpSquads_Users_Dps2UserId", column => column.Dps2UserId, "Users", "Id");
                                                      table.ForeignKey("FK_RaidAppointmentLineUpSquads_Users_Dps3UserId", column => column.Dps3UserId, "Users", "Id");
                                                      table.ForeignKey("FK_RaidAppointmentLineUpSquads_Users_Dps4UserId", column => column.Dps4UserId, "Users", "Id");
                                                      table.ForeignKey("FK_RaidAppointmentLineUpSquads_Users_Dps5UserId", column => column.Dps5UserId, "Users", "Id");
                                                      table.ForeignKey("FK_RaidAppointmentLineUpSquads_Users_Dps6UserId", column => column.Dps6UserId, "Users", "Id");
                                                      table.ForeignKey("FK_RaidAppointmentLineUpSquads_Users_HealerUserId", column => column.HealerUserId, "Users", "Id");
                                                      table.ForeignKey("FK_RaidAppointmentLineUpSquads_Users_Support1UserId", column => column.Support1UserId, "Users", "Id");
                                                      table.ForeignKey("FK_RaidAppointmentLineUpSquads_Users_Support2UserId", column => column.Support2UserId, "Users", "Id");
                                                      table.ForeignKey("FK_RaidAppointmentLineUpSquads_Users_TankUserId", column => column.TankUserId, "Users", "Id");
                                                  });

        migrationBuilder.CreateIndex("IX_RaidAppointmentLineUpSquads_Dps1UserId", "RaidAppointmentLineUpSquads", "Dps1UserId");
        migrationBuilder.CreateIndex("IX_RaidAppointmentLineUpSquads_Dps2UserId", "RaidAppointmentLineUpSquads", "Dps2UserId");
        migrationBuilder.CreateIndex("IX_RaidAppointmentLineUpSquads_Dps3UserId", "RaidAppointmentLineUpSquads", "Dps3UserId");
        migrationBuilder.CreateIndex("IX_RaidAppointmentLineUpSquads_Dps4UserId", "RaidAppointmentLineUpSquads", "Dps4UserId");
        migrationBuilder.CreateIndex("IX_RaidAppointmentLineUpSquads_Dps5UserId", "RaidAppointmentLineUpSquads", "Dps5UserId");
        migrationBuilder.CreateIndex("IX_RaidAppointmentLineUpSquads_Dps6UserId", "RaidAppointmentLineUpSquads", "Dps6UserId");
        migrationBuilder.CreateIndex("IX_RaidAppointmentLineUpSquads_HealerUserId", "RaidAppointmentLineUpSquads", "HealerUserId");
        migrationBuilder.CreateIndex("IX_RaidAppointmentLineUpSquads_Support1UserId", "RaidAppointmentLineUpSquads", "Support1UserId");
        migrationBuilder.CreateIndex("IX_RaidAppointmentLineUpSquads_Support2UserId", "RaidAppointmentLineUpSquads", "Support2UserId");
        migrationBuilder.CreateIndex("IX_RaidAppointmentLineUpSquads_TankUserId", "RaidAppointmentLineUpSquads", "TankUserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("RaidAppointmentLineUpSquads");
    }

    #endregion // Migration
}