using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCombatAndSpellEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttackAction_Characters_CharacterId",
                table: "AttackAction");

            migrationBuilder.DropForeignKey(
                name: "FK_AttackDamage_AttackAction_AttackActionId",
                table: "AttackDamage");

            migrationBuilder.DropForeignKey(
                name: "FK_SpellSlot_Characters_CharacterId",
                table: "SpellSlot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SpellSlot",
                table: "SpellSlot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AttackDamage",
                table: "AttackDamage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AttackAction",
                table: "AttackAction");

            migrationBuilder.RenameTable(
                name: "SpellSlot",
                newName: "SpellSlots");

            migrationBuilder.RenameTable(
                name: "AttackDamage",
                newName: "AttackDamages");

            migrationBuilder.RenameTable(
                name: "AttackAction",
                newName: "AttackActions");

            migrationBuilder.RenameIndex(
                name: "IX_SpellSlot_CharacterId",
                table: "SpellSlots",
                newName: "IX_SpellSlots_CharacterId");

            migrationBuilder.RenameIndex(
                name: "IX_AttackDamage_AttackActionId",
                table: "AttackDamages",
                newName: "IX_AttackDamages_AttackActionId");

            migrationBuilder.RenameIndex(
                name: "IX_AttackAction_CharacterId",
                table: "AttackActions",
                newName: "IX_AttackActions_CharacterId");

            migrationBuilder.AddColumn<Guid>(
                name: "SpellId",
                table: "AttackActions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SpellSlots",
                table: "SpellSlots",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AttackDamages",
                table: "AttackDamages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AttackActions",
                table: "AttackActions",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Spells",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    RequiresSave = table.Column<bool>(type: "boolean", nullable: false),
                    SaveStat = table.Column<int>(type: "integer", nullable: true),
                    DamageDice = table.Column<string>(type: "text", nullable: false),
                    DamageType = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    BuffDebuffNotes = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spells", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttackActions_SpellId",
                table: "AttackActions",
                column: "SpellId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttackActions_Characters_CharacterId",
                table: "AttackActions",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AttackActions_Spells_SpellId",
                table: "AttackActions",
                column: "SpellId",
                principalTable: "Spells",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AttackDamages_AttackActions_AttackActionId",
                table: "AttackDamages",
                column: "AttackActionId",
                principalTable: "AttackActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SpellSlots_Characters_CharacterId",
                table: "SpellSlots",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttackActions_Characters_CharacterId",
                table: "AttackActions");

            migrationBuilder.DropForeignKey(
                name: "FK_AttackActions_Spells_SpellId",
                table: "AttackActions");

            migrationBuilder.DropForeignKey(
                name: "FK_AttackDamages_AttackActions_AttackActionId",
                table: "AttackDamages");

            migrationBuilder.DropForeignKey(
                name: "FK_SpellSlots_Characters_CharacterId",
                table: "SpellSlots");

            migrationBuilder.DropTable(
                name: "Spells");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SpellSlots",
                table: "SpellSlots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AttackDamages",
                table: "AttackDamages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AttackActions",
                table: "AttackActions");

            migrationBuilder.DropIndex(
                name: "IX_AttackActions_SpellId",
                table: "AttackActions");

            migrationBuilder.DropColumn(
                name: "SpellId",
                table: "AttackActions");

            migrationBuilder.RenameTable(
                name: "SpellSlots",
                newName: "SpellSlot");

            migrationBuilder.RenameTable(
                name: "AttackDamages",
                newName: "AttackDamage");

            migrationBuilder.RenameTable(
                name: "AttackActions",
                newName: "AttackAction");

            migrationBuilder.RenameIndex(
                name: "IX_SpellSlots_CharacterId",
                table: "SpellSlot",
                newName: "IX_SpellSlot_CharacterId");

            migrationBuilder.RenameIndex(
                name: "IX_AttackDamages_AttackActionId",
                table: "AttackDamage",
                newName: "IX_AttackDamage_AttackActionId");

            migrationBuilder.RenameIndex(
                name: "IX_AttackActions_CharacterId",
                table: "AttackAction",
                newName: "IX_AttackAction_CharacterId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SpellSlot",
                table: "SpellSlot",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AttackDamage",
                table: "AttackDamage",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AttackAction",
                table: "AttackAction",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AttackAction_Characters_CharacterId",
                table: "AttackAction",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AttackDamage_AttackAction_AttackActionId",
                table: "AttackDamage",
                column: "AttackActionId",
                principalTable: "AttackAction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SpellSlot_Characters_CharacterId",
                table: "SpellSlot",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
