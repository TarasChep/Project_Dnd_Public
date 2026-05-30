using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnD.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    CurrentXp = table.Column<int>(type: "integer", nullable: false),
                    Race = table.Column<string>(type: "text", nullable: false),
                    Class = table.Column<string>(type: "text", nullable: false),
                    Background = table.Column<string>(type: "text", nullable: false),
                    Strength = table.Column<int>(type: "integer", nullable: false),
                    Dexterity = table.Column<int>(type: "integer", nullable: false),
                    Constitution = table.Column<int>(type: "integer", nullable: false),
                    Intelligence = table.Column<int>(type: "integer", nullable: false),
                    Wisdom = table.Column<int>(type: "integer", nullable: false),
                    Charisma = table.Column<int>(type: "integer", nullable: false),
                    CurrentHp = table.Column<int>(type: "integer", nullable: false),
                    MaxHp = table.Column<int>(type: "integer", nullable: false),
                    ArmorClass = table.Column<int>(type: "integer", nullable: false),
                    Speed = table.Column<int>(type: "integer", nullable: false),
                    Inventory = table.Column<List<string>>(type: "text[]", nullable: false),
                    Spells = table.Column<List<string>>(type: "text[]", nullable: false),
                    Feats = table.Column<List<string>>(type: "text[]", nullable: false),
                    ClassFeatures = table.Column<List<string>>(type: "text[]", nullable: false),
                    RacialTraits = table.Column<List<string>>(type: "text[]", nullable: false),
                    AppearanceDescription = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    Athletics = table.Column<int>(type: "integer", nullable: false),
                    Acrobatics = table.Column<int>(type: "integer", nullable: false),
                    SleightOfHand = table.Column<int>(type: "integer", nullable: false),
                    Stealth = table.Column<int>(type: "integer", nullable: false),
                    Arcana = table.Column<int>(type: "integer", nullable: false),
                    History = table.Column<int>(type: "integer", nullable: false),
                    Investigation = table.Column<int>(type: "integer", nullable: false),
                    Nature = table.Column<int>(type: "integer", nullable: false),
                    Religion = table.Column<int>(type: "integer", nullable: false),
                    AnimalHandling = table.Column<int>(type: "integer", nullable: false),
                    Insight = table.Column<int>(type: "integer", nullable: false),
                    Medicine = table.Column<int>(type: "integer", nullable: false),
                    Perception = table.Column<int>(type: "integer", nullable: false),
                    Survival = table.Column<int>(type: "integer", nullable: false),
                    Deception = table.Column<int>(type: "integer", nullable: false),
                    Intimidation = table.Column<int>(type: "integer", nullable: false),
                    Performance = table.Column<int>(type: "integer", nullable: false),
                    Persuasion = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Characters");
        }
    }
}
