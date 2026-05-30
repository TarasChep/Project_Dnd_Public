using DnD.Domain.Enums;

namespace DnD.Domain.Entities;

public class ResourceTracker : BaseEntity
{
    // Foreign Key для БД
    public Guid CharacterId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CurrentValue { get; set; }
    public int MaxValue { get; set; }

    // Умова відновлення (ShortRest, LongRest, None)
    public ResetCondition ResetCondition { get; set; }

    // Навігаційна властивість для EF Core (не створює колонку в БД)
    public Character Character { get; set; } = null!;
}
