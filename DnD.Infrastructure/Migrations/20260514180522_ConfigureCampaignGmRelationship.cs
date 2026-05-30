using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureCampaignGmRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_AspNetUsers_GMId",
                table: "Campaigns");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_GMId",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "GMId",
                table: "Campaigns");

            migrationBuilder.AddColumn<bool>(
                name: "IsVisibleToAllPlayers",
                table: "CampaignCharacters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<List<Guid>>(
                name: "VisibleToUserIds",
                table: "CampaignCharacters",
                type: "uuid[]",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_GmUserId",
                table: "Campaigns",
                column: "GmUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_AspNetUsers_GmUserId",
                table: "Campaigns",
                column: "GmUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_AspNetUsers_GmUserId",
                table: "Campaigns");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_GmUserId",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "IsVisibleToAllPlayers",
                table: "CampaignCharacters");

            migrationBuilder.DropColumn(
                name: "VisibleToUserIds",
                table: "CampaignCharacters");

            migrationBuilder.AddColumn<Guid>(
                name: "GMId",
                table: "Campaigns",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_GMId",
                table: "Campaigns",
                column: "GMId");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_AspNetUsers_GMId",
                table: "Campaigns",
                column: "GMId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
