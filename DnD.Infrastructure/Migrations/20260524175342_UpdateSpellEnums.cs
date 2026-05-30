using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSpellEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AoESizeFeet",
                table: "Spells",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HalfOnSuccess",
                table: "Spells",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAoE",
                table: "Spells",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "School",
                table: "Spells",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Shape",
                table: "Spells",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Spells",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "AoESizeFeet", "HalfOnSuccess", "IsAoE", "School", "Shape" },
                values: new object[] { 0, false, false, 4, 0 });

            migrationBuilder.UpdateData(
                table: "Spells",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "AoESizeFeet", "HalfOnSuccess", "IsAoE", "School", "Shape" },
                values: new object[] { 0, false, false, 4, 0 });

            migrationBuilder.UpdateData(
                table: "Spells",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "AoESizeFeet", "HalfOnSuccess", "IsAoE", "School", "Shape" },
                values: new object[] { 0, false, false, 4, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AoESizeFeet",
                table: "Spells");

            migrationBuilder.DropColumn(
                name: "HalfOnSuccess",
                table: "Spells");

            migrationBuilder.DropColumn(
                name: "IsAoE",
                table: "Spells");

            migrationBuilder.DropColumn(
                name: "School",
                table: "Spells");

            migrationBuilder.DropColumn(
                name: "Shape",
                table: "Spells");
        }
    }
}
