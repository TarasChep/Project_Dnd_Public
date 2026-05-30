using DnD.Application.DTOs;
using DnD.Domain.Entities;

namespace DnD.Application.Interfaces;

public interface ICampaignEncounterService
{
    Task<Encounter> CreateEncounterAsync(CreateCampaignEncounterDto dto);
    Task<IEnumerable<EncounterParticipant>> BulkAddParticipantsAsync(
        Guid encounterId,
        BulkAddParticipantsDto dto
    );

    Task StartEncounterAsync(Guid encounterId);

    Task UpdateInitiativeAsync(Guid participantId, int roll);
    Task NextTurnAsync(Guid encounterId);
}
