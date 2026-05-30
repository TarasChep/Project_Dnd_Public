using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DnD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedSpells : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Spells",
                columns: new[] { "Id", "BuffDebuffNotes", "CreatedAt", "DamageDice", "DamageType", "Description", "LastModifiedAt", "Level", "Name", "RequiresSave", "SaveStat" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "8d6", "Fire", "A bright streak flashes from your pointing finger to a point you choose within range then blossoms with a low roar into an explosion of flame.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Fireball", true, 2 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", "", "A spectral, floating hand appears at a point you choose within range. The hand lasts for the duration or until you dismiss it as an action.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, "Mage Hand", false, null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Speed reduced by 10ft", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "1d8", "Cold", "A frigid beam of blue-white light streaks toward a creature within range. Make a ranged spell attack against the target. On a hit, it takes 1d8 cold damage, and its speed is reduced by 10 feet until the start of your next turn.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, "Ray of Frost", false, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Spells",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Spells",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Spells",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));
        }
    }
}
