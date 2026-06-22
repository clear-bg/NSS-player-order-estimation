using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NssOrderTool.Migrations
{
    /// <inheritdoc />
    public partial class AddSeasonRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ArenaSessions_season_id",
                table: "ArenaSessions",
                column: "season_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ArenaSessions_Seasons_season_id",
                table: "ArenaSessions",
                column: "season_id",
                principalTable: "Seasons",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArenaSessions_Seasons_season_id",
                table: "ArenaSessions");

            migrationBuilder.DropIndex(
                name: "IX_ArenaSessions_season_id",
                table: "ArenaSessions");
        }
    }
}
