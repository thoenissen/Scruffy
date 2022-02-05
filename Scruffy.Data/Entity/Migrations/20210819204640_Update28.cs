using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 28
    /// </summary>
    public partial class Update28 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>("LastAge",
                                             "Accounts",
                                             "bigint",
                                             nullable: false,
                                             defaultValue: 0L);

            migrationBuilder.CreateTable("AccountDailyLoginChecks",
                                         table => new
                                                  {
                                                      Name = table.Column<string>("nvarchar(42)", maxLength: 42, nullable: false),
                                                      Date = table.Column<DateTime>("datetime2", nullable: false)
                                                  },
                                         constraints: table =>
                                                      {
                                                          table.PrimaryKey("PK_AccountDailyLoginChecks",
                                                                           x => new
                                                                                {
                                                                                    x.Name,
                                                                                    x.Date
                                                                                });

                                                          table.ForeignKey("FK_AccountDailyLoginChecks_Accounts_Name",
                                                                           x => x.Name,
                                                                           "Accounts",
                                                                           "Name",
                                                                           onDelete: ReferentialAction.Restrict);
                                                      });
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("AccountDailyLoginChecks");
            migrationBuilder.DropColumn("LastAge", "Accounts");
        }
    }
}