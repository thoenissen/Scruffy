using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 2
    /// </summary>
    public partial class Update2 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("WeeklyReminders",
                                         table => new
                                                  {
                                                      Id = table.Column<int>("int", nullable: false)
                                                                .Annotation("SqlServer:Identity", "1, 1"),
                                                      DayOfWeek = table.Column<int>("int", nullable: false),
                                                      PostTime = table.Column<TimeSpan>("time", nullable: false),
                                                      DeletionTime = table.Column<TimeSpan>("time", nullable: false),
                                                      ChannelId = table.Column<decimal>("decimal(20,0)", nullable: false),
                                                      Message = table.Column<string>("nvarchar(max)", nullable: true),
                                                      MessageId = table.Column<decimal>("decimal(20,0)", nullable: true)
                                                  },
                                         constraints: table => table.PrimaryKey("PK_WeeklyReminders", x => x.Id));
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("WeeklyReminders");
        }
    }
}