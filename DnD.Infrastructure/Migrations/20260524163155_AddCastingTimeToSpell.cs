using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCastingTimeToSpell : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CastingTime",
                table: "Spells",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Spells",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CastingTime",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Spells",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CastingTime",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Spells",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CastingTime",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CastingTime",
                table: "Spells");
        }
    }
}
