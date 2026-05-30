namespace DnD.Domain.Entities;

public class Encounter : BaseEntity
{
    public Guid? CampaignId { get; set; }
    public Campaign? Campaign { get; set; }

    // Додаємо UserId, щоб зіткнення без кампанії (з Аналізатора) мали власника
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int CurrentTurnIndex { get; set; }

    public ICollection<EncounterParticipant> Participants { get; set; } = new List<EncounterParticipant>();
}
