using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 46
    /// </summary>
    public partial class Update46 : Migration
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE FUNCTION GetDateRange(
                                        @from datetime2(7),
                                        @to  datetime2(7)
                                    )
                                   RETURNS TABLE
                                   AS
                                    RETURN
 
                                   WITH [DAY_RANGE] AS
                                   (
                                        SELECT @from AS[Value],
                                        1 AS[Level]
                                        
                                        UNION ALL
 
                                        SELECT DATEADD(DAY, 1, [Value]),
                                               [Level] + 1
                                          FROM [DAY_RANGE]
                                         WHERE [Value] < @to)

                                   SELECT [Value]
                                   FROM   [DAY_RANGE]");
        }

        /// <summary>
        /// Downgrade
        /// </summary>
        /// <param name="migrationBuilder">Builder</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION GetDateRange");
        }
    }
}