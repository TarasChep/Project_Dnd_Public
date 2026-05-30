using DnD.Domain.Enums;

namespace DnD.Application.DTOs;

public class CreateCampaignEncounterDto
{
    public Guid CampaignId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class BulkAddParticipantsDto
{
    public Guid CharacterId { get; set; }
    public int Count { get; set; }
    public Faction Faction { get; set; }
}

public class UpdateInitiativeDto
{
    public int Initiative { get; set; }
}