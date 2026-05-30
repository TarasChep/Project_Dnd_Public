using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEncounterModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EncounterParticipants_Characters_CharacterId",
                table: "EncounterParticipants");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Encounters",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CharacterId",
                table: "EncounterParticipants",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<int>(
                name: "MaxHp",
                table: "EncounterParticipants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Encounters_UserId",
                table: "Encounters",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EncounterParticipants_Characters_CharacterId",
                table: "EncounterParticipants",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Encounters_AspNetUsers_UserId",
                table: "Encounters",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EncounterParticipants_Characters_CharacterId",
                table: "EncounterParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_Encounters_AspNetUsers_UserId",
                table: "Encounters");

            migrationBuilder.DropIndex(
                name: "IX_Encounters_UserId",
                table: "Encounters");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Encounters");

            migrationBuilder.DropColumn(
                name: "MaxHp",
                table: "EncounterParticipants");

            migrationBuilder.AlterColumn<Guid>(
                name: "CharacterId",
                table: "EncounterParticipants",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EncounterParticipants_Characters_CharacterId",
                table: "EncounterParticipants",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
