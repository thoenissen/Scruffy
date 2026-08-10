using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scruffy.Data.Entity.Migrations;

/// <inheritdoc />
public partial class Update94 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "CoreConfigurations",
                                     columns: table => new
                                              {
                                                  Key = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                                                  Value = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true)
                                              },
                                     constraints: table =>
                                                  {
                                                      table.PrimaryKey("PK_CoreConfigurations", x => x.Key);
                                                  });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "CoreConfigurations");
    }
}