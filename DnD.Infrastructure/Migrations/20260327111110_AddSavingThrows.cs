using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSavingThrows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCharismaSaveProficient",
                table: "Characters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsConstitutionSaveProficient",
                table: "Characters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDexteritySaveProficient",
                table: "Characters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsIntelligenceSaveProficient",
                table: "Characters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStrengthSaveProficient",
                table: "Characters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWisdomSaveProficient",
                table: "Characters",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCharismaSaveProficient",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "IsConstitutionSaveProficient",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "IsDexteritySaveProficient",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "IsIntelligenceSaveProficient",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "IsStrengthSaveProficient",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "IsWisdomSaveProficient",
                table: "Characters");
        }
    }
}
