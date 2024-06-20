using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 3
    /// </summary>
    public partial class Update3 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_FractalRegistrations_FractalLfgConfigurations_FractalLfgConfigurationId", "FractalRegistrations");
            migrationBuilder.DropIndex("IX_FractalRegistrations_FractalLfgConfigurationId", "FractalRegistrations");
            migrationBuilder.DropColumn("FractalLfgConfigurationId", "FractalRegistrations");

            migrationBuilder.AddForeignKey("FK_FractalRegistrations_FractalLfgConfigurations_ConfigurationId",
                                           "FractalRegistrations",
                                           "ConfigurationId",
                                           "FractalLfgConfigurations",
                                           principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_FractalRegistrations_FractalLfgConfigurations_ConfigurationId", "FractalRegistrations");

            migrationBuilder.AddColumn<long>("FractalLfgConfigurationId",
                                             "FractalRegistrations",
                                             "bigint",
                                             nullable: false,
                                             defaultValue: 0L);
            migrationBuilder.CreateIndex("IX_FractalRegistrations_FractalLfgConfigurationId", "FractalRegistrations", "FractalLfgConfigurationId");

            migrationBuilder.AddForeignKey("FK_FractalRegistrations_FractalLfgConfigurations_FractalLfgConfigurationId",
                                           "FractalRegistrations",
                                           "FractalLfgConfigurationId",
                                           "FractalLfgConfigurations",
                                           principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);
        }
    }
}