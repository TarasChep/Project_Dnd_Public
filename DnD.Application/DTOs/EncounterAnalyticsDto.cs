using DnD.Domain.Enums;

namespace DnD.Application.DTOs;

public class CombatActionAnalyticsDto
{
    public Guid ActionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ActionCost ActionCost { get; set; }
    public bool IsSpell { get; set; }
    public bool IsSave { get; set; }
    public int SaveDC { get; set; }
    public int AttackBonus { get; set; }
    public double AverageDamage { get; set; }
    public bool IsAoE { get; set; }
    public AoEShape Shape { get; set; }
    public int AoESizeFeet { get; set; }
    public bool HalfOnSuccess { get; set; }
    public MagicSchool? School { get; set; }
}

public class ParticipantAnalyticsDto
{
    public Guid ParticipantId { get; set; }
    public Faction Faction { get; set; }
    public int CurrentHp { get; set; }
    public int ArmorClass { get; set; }
    public List<CombatActionAnalyticsDto> AvailableActions { get; set; } = new();
}

public class EncounterAnalyticsDto
{
    public List<ParticipantAnalyticsDto> Allies { get; set; } = new();
    public List<ParticipantAnalyticsDto> Enemies { get; set; } = new();
}