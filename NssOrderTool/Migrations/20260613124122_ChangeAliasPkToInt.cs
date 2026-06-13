using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NssOrderTool.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAliasPkToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Aliases",
                table: "Aliases");

            migrationBuilder.AddColumn<int>(
                name: "id",
                table: "Aliases",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Aliases",
                table: "Aliases",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Aliases",
                table: "Aliases");

            migrationBuilder.DropColumn(
                name: "id",
                table: "Aliases");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Aliases",
                table: "Aliases",
                column: "alias_name");
        }
    }
}
