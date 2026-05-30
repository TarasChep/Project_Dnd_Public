using DnD.Domain.Enums;

namespace DnD.Application.DTOs;

public class CreateAttackDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsAttackRoll { get; set; }
    public bool IsProficient { get; set; }
    public StatType? AttackStat { get; set; }
    public int FlatAttackBonus { get; set; }
    public ActionCost ActionCost { get; set; } = ActionCost.Action;
    public Guid? SpellId { get; set; }
    
    // Список шкоди (масив)
    public List<AttackDamageDto> Damages { get; set; } = new();
}