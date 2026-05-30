using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text.Json.Serialization;
using DnD.Domain.Entities;
using DnD.Domain.Enums;
using DnD.Domain.Services;

namespace DnD.Domain.Entities;

/// <summary>
/// Представляє цифровий лист персонажа (Entity), що зберігає стан,
/// характеристики та навички героя.
/// </summary>
public class Character : BaseEntity
{
    private int _currentHp;
    private int _currentXp;
    private int _maxHp;
    private int _armorClass = 10;

    //Indetefication
    public Guid UserId { get; set; } // forigeon key

    [JsonIgnore]
    public User? User { get; set; }
    public string Name { get; set; } = string.Empty;

    //Progression and class race
    public int Level { get; private set; } = 1;
    public int CurrentXp
    {
        get => _currentXp;
        set
        {
            _currentXp = value < 0 ? 0 : value;
            RecalculateLevel();
        }
    }
    public string Race { get; set; } = "Human";
    public string Class { get; set; } = "Fighter";
    public string Background { get; set; } = "Noble";
    public string Alignment { get; set; } = "True Neutral";
    public string? ImageUrl { get; set; }

    //  Main Stats
    public int Strength { get; set; } = 10;
    public int Dexterity { get; set; } = 10;
    public int Constitution { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Charisma { get; set; } = 10;

    //
    public int TemporaryHp { get; set; } = 0;
    public int CurrentHp
    {
        get => _currentHp;
        set => _currentHp = value < 0 ? 0 : value;
    }
    public int MaxHp
    {
        get => _maxHp;
        set => _maxHp = value < 0 ? 0 : value;
    }
    public int ArmorClass
    {
        get => _armorClass;
        set => _armorClass = value < 0 ? 0 : value;
    }
    public int Speed { get; set; } = 30; // Feets

    // --- ІНІЦІАТИВА (Initiative) ---
    // Initiative = DEX modifier + AdditionalInitiativeBonus
    // Це D&D 5e правило: Initiative залежить від спритності
    public int AdditionalInitiativeBonus { get; set; } = 0; // Може змінюватися через феати, предмети тощо

    [NotMapped]
    public int Initiative => DnDCalculator.CalculateModifier(Dexterity) + AdditionalInitiativeBonus;

    // --- ЕКОНОМІКА (Наші нові поля для гаманця) ---
    public int Platinum { get; set; } = 0;
    public int Gold { get; set; } = 0;
    public int Silver { get; set; } = 0;
    public int Copper { get; set; } = 0;

    //  Lists (Inventory, Spells, Feats)
    public List<string> Inventory { get; set; } = new();
    public List<string> Spells { get; set; } = new();
    public List<string> Feats { get; set; } = new();

    // --- Додаткова інформація та Нотатки ---
    /// <summary>
    /// Класові вміння (напр. Second Wind, Action Surge для воїна).
    /// </summary>
    ///
    ///
    public List<string> ClassFeatures { get; set; } = new();

    /// <summary>
    /// Расові особливості (напр. Darkvision, Fey Ancestry).
    /// </summary>
    public List<string> RacialTraits { get; set; } = new();

    /// <summary>
    /// Appearance of character(how character looks like)
    /// </summary>
    public string AppearanceDescription { get; set; } = string.Empty;

    /// <summary>
    /// Passive bonuses (e.g., resistances)
    /// </summary>
    public string Passives { get; set; } = string.Empty;

    /// <summary>
    /// Tracker notes
    /// </summary>
    public string TrackersText { get; set; } = string.Empty;

    /// <summary>
    ///  Game notes
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    // --- Saving Throws (Рятівні кидки) ---
    // Зберігаємо інформацію про те, чи має клас персонажа proficiency в цих кидках
    public bool IsStrengthSaveProficient { get; set; } = false;
    public bool IsDexteritySaveProficient { get; set; } = false;
    public bool IsConstitutionSaveProficient { get; set; } = false;
    public bool IsIntelligenceSaveProficient { get; set; } = false;
    public bool IsWisdomSaveProficient { get; set; } = false;
    public bool IsCharismaSaveProficient { get; set; } = false;

    // --- Навички (Skills) за типом SkillProficiency ---

    // Strength
    public SkillProficiency Athletics { get; set; } = SkillProficiency.None;

    //  Dexterity
    public SkillProficiency Acrobatics { get; set; } = SkillProficiency.None;
    public SkillProficiency SleightOfHand { get; set; } = SkillProficiency.None;
    public SkillProficiency Stealth { get; set; } = SkillProficiency.None;

    // Intelegence
    public SkillProficiency Arcana { get; set; } = SkillProficiency.None;
    public SkillProficiency History { get; set; } = SkillProficiency.None;
    public SkillProficiency Investigation { get; set; } = SkillProficiency.None;
    public SkillProficiency Nature { get; set; } = SkillProficiency.None;
    public SkillProficiency Religion { get; set; } = SkillProficiency.None;

    //  Wisdom
    public SkillProficiency AnimalHandling { get; set; } = SkillProficiency.None;
    public SkillProficiency Insight { get; set; } = SkillProficiency.None;
    public SkillProficiency Medicine { get; set; } = SkillProficiency.None;
    public SkillProficiency Perception { get; set; } = SkillProficiency.None;
    public SkillProficiency Survival { get; set; } = SkillProficiency.None;

    // Charisma
    public SkillProficiency Deception { get; set; } = SkillProficiency.None;
    public SkillProficiency Intimidation { get; set; } = SkillProficiency.None;
    public SkillProficiency Performance { get; set; } = SkillProficiency.None;
    public SkillProficiency Persuasion { get; set; } = SkillProficiency.None;

    [NotMapped]
    public int proficiencyBonus => DnDCalculator.CalculateProficiency(Level);

    [NotMapped]
    public int NextLevelXp => ExperienceTable.GetThreshold(Level + 1);

    // Method to get skill bonus

    // --- ВИТРИВАЛІСТЬ ТА ВІДПОЧИНОК (HIT DICE) ---
    public int HitDiceMax { get; private set; } = 1;
    public int HitDiceCurrent { get; set; } = 1;
    public DiceType HitDiceType { get; set; } = DiceType.D8; // d8 за замовчуванням

    // --- МАГІЯ ---
    public StatType? SpellcastingAbility { get; set; }

    // --- ЗВ'ЯЗКИ (RELATIONSHIPS) ---
    // EF Core автоматично підтягне всі трекери гравця сюди
    public virtual ICollection<ResourceTracker> ResourceTrackers { get; set; } =
        new List<ResourceTracker>();

    // --- АТАКИ (ATTACKS) ---
    public virtual ICollection<AttackAction> Attacks { get; set; } = new List<AttackAction>();

    // --- МАГІЯ (SPELL SLOTS - Плейсхолдери) ---
    public virtual ICollection<SpellSlot> SpellSlots { get; set; } = new List<SpellSlot>();

    // --- ІНТЕГРАЦІЇ ---
    // URL для відправки кидків у Discord-канал (Webhook)
    public string? DiscordWebhookUrl { get; set; }

    public string ThemeColor { get; set; } = "#3498DB"; // Дефолтний синій

    public int GetSkillBonus(int statValue, SkillProficiency prof)
    {
        return DnDCalculator.GetTotalSkillBonus(statValue, Level, prof);
    }

    private void RecalculateLevel()
    {
        var (newLevel, _) = ExperienceTable.CalculateLevelInfo(_currentXp);

        if (Level != newLevel)
        {
            Level = newLevel;
            HitDiceMax = newLevel; // Автоматичний скейлінг кубиків
        }
    }
}
