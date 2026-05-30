using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignsAndInitiative : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CampaignId",
                table: "Encounters",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentTurnIndex",
                table: "Encounters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "CustomName",
                table: "EncounterParticipants",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "EncounterParticipants",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "EncounterParticipants",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    GmUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    GMId = table.Column<Guid>(type: "uuid", nullable: false),
                    InviteCode = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Campaigns_AspNetUsers_GMId",
                        column: x => x.GMId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignFolders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignFolders_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignMembers",
                columns: table => new
                {
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignMembers", x => new { x.CampaignId, x.UserId });
                    table.ForeignKey(
                        name: "FK_CampaignMembers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignMembers_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignCharacters",
                columns: table => new
                {
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    FolderId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsPlayerCharacter = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignCharacters", x => new { x.CampaignId, x.CharacterId });
                    table.ForeignKey(
                        name: "FK_CampaignCharacters_CampaignFolders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "CampaignFolders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CampaignCharacters_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignCharacters_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Encounters_CampaignId",
                table: "Encounters",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignCharacters_CharacterId",
                table: "CampaignCharacters",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignCharacters_FolderId",
                table: "CampaignCharacters",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignFolders_CampaignId",
                table: "CampaignFolders",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignMembers_UserId",
                table: "CampaignMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_GMId",
                table: "Campaigns",
                column: "GMId");

            migrationBuilder.AddForeignKey(
                name: "FK_Encounters_Campaigns_CampaignId",
                table: "Encounters",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Encounters_Campaigns_CampaignId",
                table: "Encounters");

            migrationBuilder.DropTable(
                name: "CampaignCharacters");

            migrationBuilder.DropTable(
                name: "CampaignMembers");

            migrationBuilder.DropTable(
                name: "CampaignFolders");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropIndex(
                name: "IX_Encounters_CampaignId",
                table: "Encounters");

            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "Encounters");

            migrationBuilder.DropColumn(
                name: "CurrentTurnIndex",
                table: "Encounters");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "EncounterParticipants");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "EncounterParticipants");

            migrationBuilder.AlterColumn<string>(
                name: "CustomName",
                table: "EncounterParticipants",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
