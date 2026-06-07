using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NssOrderTool.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqliteCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Aliases",
                columns: table => new
                {
                    alias_name = table.Column<string>(type: "TEXT", nullable: false),
                    target_player_id = table.Column<string>(type: "TEXT", nullable: false),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aliases", x => x.alias_name);
                });

            migrationBuilder.CreateTable(
                name: "ArenaSessions",
                columns: table => new
                {
                    session_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArenaSessions", x => x.session_id);
                });

            migrationBuilder.CreateTable(
                name: "Observations",
                columns: table => new
                {
                    observation_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    observation_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Observations", x => x.observation_id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    player_id = table.Column<string>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: true),
                    first_seen = table.Column<DateTime>(type: "TEXT", nullable: false),
                    rate_mean = table.Column<double>(type: "REAL", nullable: false),
                    rate_sigma = table.Column<double>(type: "REAL", nullable: false),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.player_id);
                });

            migrationBuilder.CreateTable(
                name: "RateHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerId = table.Column<string>(type: "TEXT", nullable: false),
                    Rate = table.Column<double>(type: "REAL", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RateHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SequencePairs",
                columns: table => new
                {
                    predecessor_id = table.Column<string>(type: "TEXT", nullable: false),
                    successor_id = table.Column<string>(type: "TEXT", nullable: false),
                    frequency = table.Column<int>(type: "INTEGER", nullable: false),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SequencePairs", x => new { x.predecessor_id, x.successor_id });
                });

            migrationBuilder.CreateTable(
                name: "ArenaRounds",
                columns: table => new
                {
                    round_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    session_id = table.Column<int>(type: "INTEGER", nullable: false),
                    round_number = table.Column<int>(type: "INTEGER", nullable: false),
                    winning_team = table.Column<int>(type: "INTEGER", nullable: false),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArenaRounds", x => x.round_id);
                    table.ForeignKey(
                        name: "FK_ArenaRounds_ArenaSessions_session_id",
                        column: x => x.session_id,
                        principalTable: "ArenaSessions",
                        principalColumn: "session_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArenaParticipants",
                columns: table => new
                {
                    participant_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    session_id = table.Column<int>(type: "INTEGER", nullable: false),
                    player_id = table.Column<string>(type: "TEXT", nullable: false),
                    slot_index = table.Column<int>(type: "INTEGER", nullable: false),
                    win_count = table.Column<int>(type: "INTEGER", nullable: false),
                    rank = table.Column<int>(type: "INTEGER", nullable: false),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArenaParticipants", x => x.participant_id);
                    table.ForeignKey(
                        name: "FK_ArenaParticipants_ArenaSessions_session_id",
                        column: x => x.session_id,
                        principalTable: "ArenaSessions",
                        principalColumn: "session_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArenaParticipants_Players_player_id",
                        column: x => x.player_id,
                        principalTable: "Players",
                        principalColumn: "player_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ObservationDetails",
                columns: table => new
                {
                    detail_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    observation_id = table.Column<int>(type: "INTEGER", nullable: false),
                    player_id = table.Column<string>(type: "TEXT", nullable: false),
                    order_index = table.Column<int>(type: "INTEGER", nullable: false),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObservationDetails", x => x.detail_id);
                    table.ForeignKey(
                        name: "FK_ObservationDetails_Observations_observation_id",
                        column: x => x.observation_id,
                        principalTable: "Observations",
                        principalColumn: "observation_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ObservationDetails_Players_player_id",
                        column: x => x.player_id,
                        principalTable: "Players",
                        principalColumn: "player_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArenaParticipants_player_id",
                table: "ArenaParticipants",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_ArenaParticipants_session_id",
                table: "ArenaParticipants",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_ArenaRounds_session_id",
                table: "ArenaRounds",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_ObservationDetails_observation_id",
                table: "ObservationDetails",
                column: "observation_id");

            migrationBuilder.CreateIndex(
                name: "IX_ObservationDetails_player_id",
                table: "ObservationDetails",
                column: "player_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aliases");

            migrationBuilder.DropTable(
                name: "ArenaParticipants");

            migrationBuilder.DropTable(
                name: "ArenaRounds");

            migrationBuilder.DropTable(
                name: "ObservationDetails");

            migrationBuilder.DropTable(
                name: "RateHistories");

            migrationBuilder.DropTable(
                name: "SequencePairs");

            migrationBuilder.DropTable(
                name: "ArenaSessions");

            migrationBuilder.DropTable(
                name: "Observations");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
