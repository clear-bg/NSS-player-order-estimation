using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NssOrderTool.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeObservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ordered_list",
                table: "Observations");

            migrationBuilder.CreateTable(
                name: "ObservationDetails",
                columns: table => new
                {
                    detail_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    observation_id = table.Column<int>(type: "int", nullable: false),
                    player_id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    order_index = table.Column<int>(type: "int", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                name: "ObservationDetails");

            migrationBuilder.AddColumn<string>(
                name: "ordered_list",
                table: "Observations",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
