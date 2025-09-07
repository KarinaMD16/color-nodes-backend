using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace color_nodes_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddGameAndGameMove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameMoves",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false),
                    FromIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    ToIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameMoves", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoomCode = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Cups = table.Column<string>(type: "TEXT", nullable: false),
                    TargetPattern = table.Column<string>(type: "TEXT", nullable: false),
                    PlayerOrder = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentPlayerIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    MovesThisTurn = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxMovesPerTurn = table.Column<int>(type: "INTEGER", nullable: false),
                    TurnDurationSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    TurnEndsAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastHits = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalMoves = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameMoves");

            migrationBuilder.DropTable(
                name: "Games");
        }
    }
}
