using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 68
    /// </summary>
    public partial class Update68 : Migration
    {
        #region Methods

        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>("MemberDiscordRoleId", "Guilds", "decimal(20,0)", true);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("MemberDiscordRoleId", "Guilds");
        }

        #endregion // Methods
    }
}