using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NssOrderTool.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePrimaryKeyToUUID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Aliases",
                table: "Aliases");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Players",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alias_id",
                table: "Aliases",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Aliases",
                table: "Aliases",
                column: "alias_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Aliases",
                table: "Aliases");

            migrationBuilder.DropColumn(
                name: "alias_id",
                table: "Aliases");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Players",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Aliases",
                table: "Aliases",
                column: "alias_name");
        }
    }
}
