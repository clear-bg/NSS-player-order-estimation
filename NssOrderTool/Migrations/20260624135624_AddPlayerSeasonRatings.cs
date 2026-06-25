using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NssOrderTool.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerSeasonRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerSeasonRatings",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    player_id = table.Column<string>(type: "TEXT", nullable: false),
                    season_id = table.Column<int>(type: "INTEGER", nullable: false),
                    rate_mean = table.Column<double>(type: "REAL", nullable: false),
                    rate_sigma = table.Column<double>(type: "REAL", nullable: false),
                    total_matches = table.Column<int>(type: "INTEGER", nullable: false),
                    total_wins = table.Column<int>(type: "INTEGER", nullable: false),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSeasonRatings", x => x.id);
                    table.ForeignKey(
                        name: "FK_PlayerSeasonRatings_Players_player_id",
                        column: x => x.player_id,
                        principalTable: "Players",
                        principalColumn: "player_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerSeasonRatings_Seasons_season_id",
                        column: x => x.season_id,
                        principalTable: "Seasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSeasonRatings_player_id",
                table: "PlayerSeasonRatings",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSeasonRatings_season_id",
                table: "PlayerSeasonRatings",
                column: "season_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerSeasonRatings");
        }
    }
}
