using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSpellcastingAbility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SpellcastingAbility",
                table: "Characters",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpellcastingAbility",
                table: "Characters");
        }
    }
}
