using DnD.Domain.Enums;

namespace DnD.Application.DTOs;

public class ParticipantDetailDto
{
    public Guid Id { get; set; }
    public Guid? CharacterId { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public Faction Faction { get; set; }
    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public string? CustomName { get; set; }
    public int InitiativeRoll { get; set; }
}