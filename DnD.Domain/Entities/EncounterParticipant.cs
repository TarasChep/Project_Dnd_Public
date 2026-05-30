using DnD.Domain.Enums;

namespace DnD.Domain.Entities;

public class EncounterParticipant : BaseEntity
{
    public Guid EncounterId { get; set; }
    public Encounter Encounter { get; set; } = null!;

    public Guid? CharacterId { get; set; }
    public Character? Character { get; set; }
    public Faction Faction { get; set; }
    public string CustomName { get; set; } = string.Empty;
    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public int InitiativeRoll { get; set; }
}