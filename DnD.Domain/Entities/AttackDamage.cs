using System.Text.Json.Serialization;
using DnD.Domain.Enums;

namespace DnD.Domain.Entities;

public class AttackDamage : BaseEntity
{
    public Guid AttackActionId { get; set; }

    [JsonIgnore]
    public AttackAction? AttackAction { get; set; }

    // --- ПАРАМЕТРИ ШКОДИ (DAMAGE) ---
    public DiceType DiceType { get; set; } = DiceType.D8; // Яким кубиком б'ємо
    public int DiceCount { get; set; } = 1; // Скільки кубиків (наприклад, 2d6 для двуручного меча)

    public StatType? ModifierStat { get; set; } // Який стат додаємо до шкоди (зазвичай той самий, що й для попадання, але іноді null)
    public int FlatDamageBonus { get; set; } = 0; // Магічний бонус або фіт (наприклад, Dueling style +2)

    public string DamageType { get; set; } = "Slashing"; // Тип шкоди (Fire, Piercing, Bludgeoning)
}
