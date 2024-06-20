using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 8
    /// </summary>
    public partial class Update8 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("RaidDayTemplates",
                                         table => new
                                                  {
                                                      Id = table.Column<long>("bigint", nullable: false)
                                                                .Annotation("SqlServer:Identity", "1, 1"),
                                                      AliasName = table.Column<string>("nvarchar(max)", nullable: true),
                                                      Title = table.Column<string>("nvarchar(max)", nullable: true),
                                                      Description = table.Column<string>("nvarchar(max)", nullable: true),
                                                      Thumbnail = table.Column<string>("nvarchar(max)", nullable: true),
                                                      IsDeleted = table.Column<bool>("bit", nullable: false)
                                                  },
                                         constraints: table => table.PrimaryKey("PK_RaidDayTemplates", x => x.Id));
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("RaidDayTemplates");
        }
    }
}