using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NssOrderTool.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeArenaTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "player_ids_csv",
                table: "ArenaSessions");

            migrationBuilder.CreateTable(
                name: "ArenaParticipants",
                columns: table => new
                {
                    participant_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    session_id = table.Column<int>(type: "int", nullable: false),
                    player_id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    slot_index = table.Column<int>(type: "int", nullable: false),
                    win_count = table.Column<int>(type: "int", nullable: false),
                    rank = table.Column<int>(type: "int", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ArenaParticipants_player_id",
                table: "ArenaParticipants",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_ArenaParticipants_session_id",
                table: "ArenaParticipants",
                column: "session_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArenaParticipants");

            migrationBuilder.AddColumn<string>(
                name: "player_ids_csv",
                table: "ArenaSessions",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
