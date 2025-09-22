using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace color_nodes_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomScoreToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Room_Score",
                table: "Users",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Room_Score",
                table: "Users");
        }
    }
}
