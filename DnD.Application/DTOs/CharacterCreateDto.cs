using DnD.Domain.Enums;

namespace DnD.Application.DTOs;

public class CharacterCreateDto
{
    public string? ImageUrl { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Race { get; set; } = "Human";
    public string Class { get; set; } = "Fighter";
    public string Background { get; set; } = "Noble";
    public string Alignment { get; set; } = "True Neutral";

    public int CurrentXp { get; set; } = 0;

    public int MaxHp { get; set; } = 10;
    public int ArmorClass { get; set; } = 10;
    public int CurrentHp { get; set; } = 10;
    public DiceType HitDiceType { get; set; } = DiceType.D8;

    // Базові характеристики
    public int Strength { get; set; } = 10;
    public int Dexterity { get; set; } = 10;
    public int Constitution { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Charisma { get; set; } = 10;

    // --- Списки (Спорядження, Здібності) ---
    public List<string> Inventory { get; set; } = new();
    public List<string> Spells { get; set; } = new();
    public List<string> Feats { get; set; } = new();
    public List<string> ClassFeatures { get; set; } = new();
    public List<string> RacialTraits { get; set; } = new();

    // --- Описи ---
    public string AppearanceDescription { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    // --- Saving Throws (Рятівні кидки) ---
    // Зберігаємо інформацію про те, чи має клас персонажа proficiency в цих кидках
    public bool IsStrengthSaveProficient { get; set; } = false;
    public bool IsDexteritySaveProficient { get; set; } = false;
    public bool IsConstitutionSaveProficient { get; set; } = false;
    public bool IsIntelligenceSaveProficient { get; set; } = false;
    public bool IsWisdomSaveProficient { get; set; } = false;
    public bool IsCharismaSaveProficient { get; set; } = false;

    // --- Навички (Skills) ---
    public SkillProficiency Athletics { get; set; } = SkillProficiency.None;
    public SkillProficiency Acrobatics { get; set; } = SkillProficiency.None;
    public SkillProficiency SleightOfHand { get; set; } = SkillProficiency.None;
    public SkillProficiency Stealth { get; set; } = SkillProficiency.None;
    public SkillProficiency Arcana { get; set; } = SkillProficiency.None;
    public SkillProficiency History { get; set; } = SkillProficiency.None;
    public SkillProficiency Investigation { get; set; } = SkillProficiency.None;
    public SkillProficiency Nature { get; set; } = SkillProficiency.None;
    public SkillProficiency Religion { get; set; } = SkillProficiency.None;
    public SkillProficiency AnimalHandling { get; set; } = SkillProficiency.None;
    public SkillProficiency Insight { get; set; } = SkillProficiency.None;
    public SkillProficiency Medicine { get; set; } = SkillProficiency.None;
    public SkillProficiency Perception { get; set; } = SkillProficiency.None;
    public SkillProficiency Survival { get; set; } = SkillProficiency.None;
    public SkillProficiency Deception { get; set; } = SkillProficiency.None;
    public SkillProficiency Intimidation { get; set; } = SkillProficiency.None;
    public SkillProficiency Performance { get; set; } = SkillProficiency.None;
    public SkillProficiency Persuasion { get; set; } = SkillProficiency.None;

    // --- ІНІЦІАТИВА (Initiative) ---
    public int AdditionalInitiativeBonus { get; set; } = 0;
}
