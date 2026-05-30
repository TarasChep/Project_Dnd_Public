using DnD.Domain.Enums;

namespace DnD.Domain.Entities;

public class Spell : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public bool RequiresSave { get; set; }
    public StatType? SaveStat { get; set; }
    public ActionCost CastingTime { get; set; } = ActionCost.Action;
    public string DamageDice { get; set; } = string.Empty; // e.g. "8d6"
    public string DamageType { get; set; } = string.Empty; // e.g. "Fire"
    public string Description { get; set; } = string.Empty;
    public string BuffDebuffNotes { get; set; } = string.Empty;
    public MagicSchool School { get; set; } = MagicSchool.Evocation;
    
    public bool IsAoE { get; set; }
    public AoEShape Shape { get; set; } = AoEShape.None;
    public int AoESizeFeet { get; set; }
    public bool HalfOnSuccess { get; set; }
}