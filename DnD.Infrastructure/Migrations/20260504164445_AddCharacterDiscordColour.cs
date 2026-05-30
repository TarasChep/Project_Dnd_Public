using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterDiscordColour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiscordWebhookUrl",
                table: "Characters",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThemeColor",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscordWebhookUrl",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "ThemeColor",
                table: "Characters");
        }
    }
}
