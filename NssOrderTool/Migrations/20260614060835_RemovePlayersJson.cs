using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NssOrderTool.Migrations
{
    /// <inheritdoc />
    public partial class RemovePlayersJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayersJson",
                table: "ArenaSessions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlayersJson",
                table: "ArenaSessions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
