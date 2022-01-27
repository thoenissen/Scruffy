
using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 1
    /// </summary>
    public partial class Update1 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("FractalLfgConfigurations",
                                         table => new
                                                  {
                                                      Id = table.Column<long>("bigint", nullable: false)
                                                                .Annotation("SqlServer:Identity", "1, 1"),
                                                      ChannelId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      MessageId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      AliasName = table.Column<string>("nvarchar(20)", maxLength: 20, nullable: true),
                                                      Title = table.Column<string>("nvarchar(max)", nullable: true),
                                                      Description = table.Column<string>("nvarchar(max)", nullable: true)
                                                  },
                                         constraints: table => table.PrimaryKey("PK_FractalLfgConfigurations", x => x.Id));

            migrationBuilder.CreateTable("FractalRegistrations",
                                         table => new
                                                  {
                                                      ConfigurationId = table.Column<long>("bigint", nullable: false),
                                                      AppointmentTimeStamp = table.Column<DateTime>("datetime2", nullable: false),
                                                      UserId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      RegistrationTimeStamp = table.Column<DateTime>("datetime2", nullable: false),
                                                      FractalLfgConfigurationId = table.Column<long>("bigint", nullable: false)
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

                                                          table.ForeignKey("FK_FractalRegistrations_FractalLfgConfigurations_FractalLfgConfigurationId",
                                                                           x => x.FractalLfgConfigurationId,
                                                                           "FractalLfgConfigurations",
                                                                           "Id",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
            migrationBuilder.CreateIndex("IX_FractalRegistrations_FractalLfgConfigurationId", "FractalRegistrations", "FractalLfgConfigurationId");
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("FractalRegistrations");
            migrationBuilder.DropTable("FractalLfgConfigurations");
        }
    }
}