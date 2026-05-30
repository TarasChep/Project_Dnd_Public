using DnD.Domain.Enums;

namespace DnD.Application.DTOs;

public class AttackActionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsAttackRoll { get; set; }
    public bool IsProficient { get; set; }
    public StatType? AttackStat { get; set; }
    public int FlatAttackBonus { get; set; }
    public ActionCost ActionCost { get; set; }
    public Guid? SpellId { get; set; }
    public SpellDto? Spell { get; set; }
    public int? SaveDC { get; set; }

    // Перевикористовуємо твій AttackDamageDto
    public List<AttackDamageDto> Damages { get; set; } = new();
}

public class CharacterFullDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Race { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public int Level { get; set; }
    public int CurrentXp { get; set; }
    public int NextLevelXp { get; set; }
    public string? ImageUrl { get; set; }
    public string Alignment { get; set; } = string.Empty;
    public string? Background { get; set; }

    // --- Основні характеристики (Stats) ---
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Constitution { get; set; }
    public int Intelligence { get; set; }
    public int Wisdom { get; set; }
    public int Charisma { get; set; }

    // --- Розраховані модифікатори (Modifiers) ---
    // Фронтенд просто малює ці цифри біля основних статів
    public int StrengthModifier { get; set; }
    public int DexterityModifier { get; set; }
    public int ConstitutionModifier { get; set; }
    public int IntelligenceModifier { get; set; }
    public int WisdomModifier { get; set; }
    public int CharismaModifier { get; set; }

    // --- Бойові параметри ---
    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public int TemporaryHp { get; set; } // ДОДАТИ ЦЕ
    public int ArmorClass { get; set; }
    public int Speed { get; set; }
    public int ProficiencyBonus { get; set; }

    // --- ІНІЦІАТИВА (Initiative) ---
    public int Initiative { get; set; } // Розраховується як: DEX modifier + AdditionalInitiativeBonus
    public int AdditionalInitiativeBonus { get; set; }
    public bool CanEdit { get; set; }

    // --- ECONOMY ---
    public int Platinum { get; set; }
    public int Gold { get; set; }
    public int Silver { get; set; }
    public int Copper { get; set; }

    // Перевикористовуємо твій AttackDamageDto
    public List<AttackActionDto> Attacks { get; set; } = new();

    // ---- ----
    // --- РЕСУРСИ ТА ТРЕКЕРИ ---
    public int HitDiceMax { get; set; }
    public int HitDiceCurrent { get; set; }
    public string HitDiceType { get; set; } = string.Empty;
    public List<ResourceTrackerDto> Trackers { get; set; } = new();
    public List<SpellSlotDto> SpellSlots { get; set; } = new();

    // --- Списки та особливості ---
    public List<string> Inventory { get; set; } = new();
    public List<string> Spells { get; set; } = new();
    public List<string> Feats { get; set; } = new();
    public DnD.Domain.Enums.StatType? SpellcastingAbility { get; set; }
    public List<string> ClassFeatures { get; set; } = new();
    public List<string> RacialTraits { get; set; } = new();
    public string AppearanceDescription { get; set; } = string.Empty;
    public string Passives { get; set; } = string.Empty;
    public string TrackersText { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    // --- Saving Throws (Рятівні кидки) ---
    // Бонус кидка + інформація про proficiency
    public int StrengthSaveBonus { get; set; }
    public bool IsStrengthSaveProficient { get; set; }

    public int DexteritySaveBonus { get; set; }
    public bool IsDexteritySaveProficient { get; set; }

    public int ConstitutionSaveBonus { get; set; }
    public bool IsConstitutionSaveProficient { get; set; }

    public int IntelligenceSaveBonus { get; set; }
    public bool IsIntelligenceSaveProficient { get; set; }

    public int WisdomSaveBonus { get; set; }
    public bool IsWisdomSaveProficient { get; set; }

    public int CharismaSaveBonus { get; set; }
    public bool IsCharismaSaveProficient { get; set; }

    // --- ФІНАЛЬНІ БОНУСИ НАВИЧОК (Skills) ---
    // Кожна навичка має: calculated bonus + proficiency level

    // Strength
    public int Athletics { get; set; }
    public SkillProficiency AthleticsProficiency { get; set; }

    // Dexterity
    public int Acrobatics { get; set; }
    public SkillProficiency AcrobaticsProficiency { get; set; }

    public int SleightOfHand { get; set; }
    public SkillProficiency SleightOfHandProficiency { get; set; }

    public int Stealth { get; set; }
    public SkillProficiency StealthProficiency { get; set; }

    // Intelligence
    public int Arcana { get; set; }
    public SkillProficiency ArcanaProficiency { get; set; }

    public int History { get; set; }
    public SkillProficiency HistoryProficiency { get; set; }

    public int Investigation { get; set; }
    public SkillProficiency InvestigationProficiency { get; set; }

    public int Nature { get; set; }
    public SkillProficiency NatureProficiency { get; set; }

    public int Religion { get; set; }
    public SkillProficiency ReligionProficiency { get; set; }

    // Wisdom
    public int AnimalHandling { get; set; }
    public SkillProficiency AnimalHandlingProficiency { get; set; }

    public int Insight { get; set; }
    public SkillProficiency InsightProficiency { get; set; }

    public int Medicine { get; set; }
    public SkillProficiency MedicineProficiency { get; set; }

    public int Perception { get; set; }
    public SkillProficiency PerceptionProficiency { get; set; }

    public int Survival { get; set; }
    public SkillProficiency SurvivalProficiency { get; set; }

    // Charisma
    public int Deception { get; set; }
    public SkillProficiency DeceptionProficiency { get; set; }

    public int Intimidation { get; set; }
    public SkillProficiency IntimidationProficiency { get; set; }

    public int Performance { get; set; }
    public SkillProficiency PerformanceProficiency { get; set; }

    public int Persuasion { get; set; }
    public SkillProficiency PersuasionProficiency { get; set; }
    public string? DiscordWebhookUrl { get; set; }
    public string ThemeColor { get; set; } = string.Empty;

    // --- Метадані ---
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
}
