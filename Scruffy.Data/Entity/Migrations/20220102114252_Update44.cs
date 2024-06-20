﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace Scruffy.Data.Entity.Migrations
{
    /// <summary>
    /// Update 44
    /// </summary>
    public partial class Update44 : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_GuildRanks_GuildRanks_SuperiorId", table: "GuildRanks");
            migrationBuilder.DropIndex(name: "IX_GuildRanks_SuperiorId", table: "GuildRanks");
            migrationBuilder.DropColumn(name: "SuperiorId", table: "GuildRanks");

            migrationBuilder.AddColumn<int>(name: "Order",
                                            table: "GuildRanks",
                                            type: "int",
                                            nullable: false,
                                            defaultValue: 0);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException();
        }
    }
}