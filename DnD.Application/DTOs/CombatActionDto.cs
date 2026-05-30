using DnD.Domain.Enums;

namespace DnD.Application.DTOs;

public class CombatActionDto
{
    public Guid ActionId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool IsSpell { get; set; }
    public bool IsSave { get; set; }
    public int SaveDC { get; set; }
    public string SaveStat { get; set; } = string.Empty;
    public int AttackBonus { get; set; }
    public string DamageDice { get; set; } = string.Empty;
    public int SpellLevel { get; set; }
    public ActionCost ActionCost { get; set; }
}