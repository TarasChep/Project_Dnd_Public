using DnD.Domain.Enums;

namespace DnD.Application.DTOs;

public class AttackDamageDto
{
    public DiceType DiceType { get; set; }
    public int DiceCount { get; set; }
    public StatType? ModifierStat { get; set; }
    public int FlatDamageBonus { get; set; }
    public string DamageType { get; set; } = string.Empty;
}
