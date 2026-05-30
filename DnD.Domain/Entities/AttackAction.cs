using System.Text.Json.Serialization;
using DnD.Domain.Enums;

namespace DnD.Domain.Entities;

public class AttackAction : BaseEntity
{
    public Guid CharacterId { get; set; }

    [JsonIgnore]
    public Character? Character { get; set; }

    public string Name { get; set; } = string.Empty; // Наприклад: "Flametongue Longsword"

    // --- ПАРАМЕТРИ ПОПАДАННЯ (TO-HIT) ---
    public bool IsAttackRoll { get; set; } = true; // Якщо false - це атака типу Magic Missile (без кидка)
    public bool IsProficient { get; set; } = false; // Чи додаємо Proficiency Bonus?
    public StatType? AttackStat { get; set; } // Від якої характеристики б'ємо (Strength, Dex, Charisma)
    public int FlatAttackBonus { get; set; } = 0; // Магічний бонус зброї (наприклад, +1)
    public ActionCost ActionCost { get; set; } = ActionCost.Action; 

    public Guid? SpellId { get; set; }
    public Spell? Spell { get; set; }

    // --- ЗВ'ЯЗКИ ---
    // Одна атака може мати кілька типів шкоди (Ріжуча + Вогняна)
    public virtual ICollection<AttackDamage> Damages { get; set; } = new List<AttackDamage>();
}
