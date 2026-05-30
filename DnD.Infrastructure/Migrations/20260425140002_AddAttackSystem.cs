using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttackSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttackActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsAttackRoll = table.Column<bool>(type: "boolean", nullable: false),
                    IsProficient = table.Column<bool>(type: "boolean", nullable: false),
                    AttackStat = table.Column<int>(type: "integer", nullable: true),
                    FlatAttackBonus = table.Column<int>(type: "integer", nullable: false),
                    SpellId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttackActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttackActions_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttackDamages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttackActionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiceType = table.Column<int>(type: "integer", nullable: false),
                    DiceCount = table.Column<int>(type: "integer", nullable: false),
                    ModifierStat = table.Column<int>(type: "integer", nullable: true),
                    FlatDamageBonus = table.Column<int>(type: "integer", nullable: false),
                    DamageType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttackDamages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttackDamages_AttackActions_AttackActionId",
                        column: x => x.AttackActionId,
                        principalTable: "AttackActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttackActions_CharacterId",
                table: "AttackActions",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_AttackActions_SpellId",
                table: "AttackActions",
                column: "SpellId");

            migrationBuilder.CreateIndex(
                name: "IX_AttackDamages_AttackActionId",
                table: "AttackDamages",
                column: "AttackActionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttackActions_Spells_SpellId",
                table: "AttackActions",
                column: "SpellId",
                principalTable: "Spells",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttackActions_Spells_SpellId",
                table: "AttackActions");

            migrationBuilder.DropTable(
                name: "AttackDamages");

            migrationBuilder.DropTable(
                name: "AttackActions");
        }
    }
}
