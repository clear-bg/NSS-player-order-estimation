using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NssOrderTool.Migrations
{
    /// <inheritdoc />
    public partial class AddNewSchemaColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_played_at",
                table: "Players",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "memo",
                table: "Players",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "total_matches",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "total_wins",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "is_valid",
                table: "ArenaSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "memo",
                table: "ArenaSessions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "season_id",
                table: "ArenaSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    start_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    end_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "last_played_at",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "memo",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "total_matches",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "total_wins",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "is_valid",
                table: "ArenaSessions");

            migrationBuilder.DropColumn(
                name: "memo",
                table: "ArenaSessions");

            migrationBuilder.DropColumn(
                name: "season_id",
                table: "ArenaSessions");
        }
    }
}
