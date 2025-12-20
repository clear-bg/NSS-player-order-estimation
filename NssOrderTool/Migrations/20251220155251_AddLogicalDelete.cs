using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NssOrderTool.Migrations
{
    /// <inheritdoc />
    public partial class AddLogicalDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "SequencePairs",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "Players",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "Observations",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "ObservationDetails",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "Aliases",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "SequencePairs");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "Observations");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "ObservationDetails");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "Aliases");
        }
    }
}
