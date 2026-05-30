using DnD.Domain.Enums;

namespace DnD.Application.DTOs;

public class AddParticipantDto
{
    public Guid CharacterId { get; set; }
    public Faction Faction { get; set; } // Прийматиме "Player" або "Enemy" завдяки конвертеру
    public int? CurrentHp { get; set; }
    public string? CustomName { get; set; }
}