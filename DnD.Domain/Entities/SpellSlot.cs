using System.Text.Json.Serialization;
using DnD.Domain.Enums; // Додаємо доступ до ResetCondition

namespace DnD.Domain.Entities;

public class SpellSlot : BaseEntity
{
    public Guid CharacterId { get; set; }

    [JsonIgnore]
    public Character? Character { get; set; }

    public int Level { get; set; }

    public int MaxValue { get; set; }
    public int CurrentValue { get; set; }

    // ДОДАЄМО ЦЕ: Як саме відновлюється цей рівень слотів
    // За замовчуванням ставимо LongRest, щоб не писати це кожен раз для звичайних магів
    public ResetCondition ResetCondition { get; set; } = ResetCondition.LongRest;
}
