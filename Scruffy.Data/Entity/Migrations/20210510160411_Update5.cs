using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 5
    /// </summary>
    public partial class Update5 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("ServerConfigurationEntity",
                                         table => new
                                                  {
                                                      ServerId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      Prefix = table.Column<string>("nvarchar(max)", nullable: true)
                                                  },
                                         constraints: table => table.PrimaryKey("PK_ServerConfigurationEntity", x => x.ServerId));
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("ServerConfigurationEntity");
        }
    }
}