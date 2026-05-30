using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInitiativeBonus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdditionalInitiativeBonus",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalInitiativeBonus",
                table: "Characters");
        }
    }
}
