using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 9
    /// </summary>
    public partial class Update9 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("LogEntries",
                                         table => new
                                                  {
                                                      Id = table.Column<long>("bigint", nullable: false)
                                                                .Annotation("SqlServer:Identity", "1, 1"),
                                                      Message = table.Column<string>("nvarchar(max)", nullable: true),
                                                      QualifiedCommandName = table.Column<string>("nvarchar(max)", nullable: true)
                                                  },
                                         constraints: table => table.PrimaryKey("PK_LogEntries", x => x.Id));
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("LogEntries");
        }
    }
}