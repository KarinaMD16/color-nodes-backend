using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace color_nodes_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddActiveGameIdToRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ActiveGameId",
                table: "Rooms",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveGameId",
                table: "Rooms");
        }
    }
}
