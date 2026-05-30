using DnD.Domain.Enums;

namespace DnD.Application.DTOs;

public class SpellDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public bool RequiresSave { get; set; }
    public string? SaveStat { get; set; }
    public string CastingTime { get; set; } = string.Empty;
    public string DamageDice { get; set; } = string.Empty;
    public string DamageType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BuffDebuffNotes { get; set; } = string.Empty;
    public MagicSchool School { get; set; }
}