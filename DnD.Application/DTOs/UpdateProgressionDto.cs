using DnD.Domain.Enums;

namespace DnD.Application.DTOs;

public class UpdateProgressionDto
{
    public int? CurrentXp { get; set; }

    public string? Name { get; set; }
    public string? Race { get; set; }
    public string? Passives { get; set; }
    public string? TrackersText { get; set; }
    public string? Class { get; set; }
    public DiceType? HitDiceType { get; set; }

    // --- Основні Характеристики ---
    public int? Strength { get; set; }
    public int? Dexterity { get; set; }
    public int? Constitution { get; set; }
    public int? Intelligence { get; set; }
    public int? Wisdom { get; set; }
    public int? Charisma { get; set; }

    // --- Рятівні кидки (Saving Throws) ---
    public bool? IsStrengthSaveProficient { get; set; }
    public bool? IsDexteritySaveProficient { get; set; }
    public bool? IsConstitutionSaveProficient { get; set; }
    public bool? IsIntelligenceSaveProficient { get; set; }
    public bool? IsWisdomSaveProficient { get; set; }
    public bool? IsCharismaSaveProficient { get; set; }

    // --- Навички (Skills) ---
    public SkillProficiency? Athletics { get; set; }
    public SkillProficiency? Acrobatics { get; set; }
    public SkillProficiency? SleightOfHand { get; set; }
    public SkillProficiency? Stealth { get; set; }
    public SkillProficiency? Arcana { get; set; }
    public SkillProficiency? History { get; set; }
    public SkillProficiency? Investigation { get; set; }
    public SkillProficiency? Nature { get; set; }
    public SkillProficiency? Religion { get; set; }
    public SkillProficiency? AnimalHandling { get; set; }
    public SkillProficiency? Insight { get; set; }
    public SkillProficiency? Medicine { get; set; }
    public SkillProficiency? Perception { get; set; }
    public SkillProficiency? Survival { get; set; }
    public SkillProficiency? Deception { get; set; }
    public SkillProficiency? Intimidation { get; set; }
    public SkillProficiency? Performance { get; set; }
    public SkillProficiency? Persuasion { get; set; }

    public DnD.Domain.Enums.StatType? SpellcastingAbility { get; set; }

    // --- ІНІЦІАТИВА (Initiative) ---
    public int? AdditionalInitiativeBonus { get; set; }

    public int? Speed { get; set; }
    public int? ArmorClass { get; set; }
}
